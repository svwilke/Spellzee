namespace RetroBlitInternal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Tilemap subsystem
    /// </summary>
    public partial class RetroBlitTilemap
    {
        /// <summary>
        /// Magic number to quickly verify that this is a valid RetroBlit file
        /// </summary>
        public const ushort RetroBlit_TMX_MAGIC = 0x0FE5;

        /// <summary>
        /// Version
        /// </summary>
        public const ushort RetroBlit_TMX_VERSION = 0x0001;

        /// <summary>
        /// Declares that this TMX file represents a map
        /// </summary>
        public const byte RetroBlit_TMX_TYPE_MAP = 0x01;

        /// <summary>
        /// Declares a tile layer section in file
        /// </summary>
        public const byte RetroBlit_TMX_SECTION_TILE_LAYER = 0x01;

        /// <summary>
        /// Declares an object group section in file
        /// </summary>
        public const byte RetroBlit_TMX_SECTION_OBJECTGROUP = 0x02;

        /// <summary>
        /// Declares TSX properties in file
        /// </summary>
        public const byte RetroBlit_TSX_PROPERTIES = 0x03;

        /// <summary>
        /// Declares the end of TMX file
        /// </summary>
        public const byte RetroBlit_TMX_SECTION_END = 0xFF;

        private FastString mWorkStr = new FastString(2048);

        /// <summary>
        /// Load a map definition from a parsed binary TMX file
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>Map definition</returns>
        public TMXMap LoadTMX(string fileName)
        {
            var map = new TMXMapDef(fileName);

            if (fileName == null)
            {
                return null;
            }

            map.realPathName = Path.GetDirectoryName(fileName) + "/" + Path.GetFileNameWithoutExtension(fileName) + ".tmx.rb/";
            map.realPathName = map.realPathName.Replace('\\', '/');

            string infoFileName = map.realPathName + "info";
            var tmxFile = Resources.Load<TextAsset>(infoFileName);

            if (tmxFile == null)
            {
                Debug.Log("Can't find TMX map at " + fileName + ". If TMX file exists then please try re-importing your TMX file.");
                return null;
            }

            try
            {
                var reader = new BinaryReader(new MemoryStream(tmxFile.bytes));

                var magicNum = reader.ReadUInt16();
                var version = reader.ReadUInt16();

                if (magicNum != RetroBlit_TMX_MAGIC)
                {
                    Debug.Log(fileName + " is not a TMX file");
                    Debug.Log("Magic: " + magicNum + " expected " + RetroBlit_TMX_MAGIC);
                    return null;
                }

                if (version > RetroBlit_TMX_VERSION)
                {
                    Debug.Log(fileName + " is of a newer version than this version of RetroBlit supports, try reimporting your TMX file into Unity.");
                    return null;
                }

                byte type = reader.ReadByte();

                if (type != RetroBlit_TMX_TYPE_MAP)
                {
                    Debug.Log(fileName + " is a RetroBlit TMX file but it is of the wrong type.");
                    return null;
                }

                int mapWidth = reader.ReadInt32();
                int mapHeight = reader.ReadInt32();
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte a = reader.ReadByte();
                bool infinite = reader.ReadBoolean();

                map.SetSize(new Vector2i(mapWidth, mapHeight));
                map.SetBackgroundColor(new Color32(r, g, b, a));
                map.SetInfinite(infinite);

                int chunkWidth = reader.ReadInt32();
                int chunkHeight = reader.ReadInt32();

                map.chunkSize = new Vector2i(chunkWidth, chunkHeight);

                // Load properties if available
                bool propsAvailable = reader.ReadBoolean();
                if (propsAvailable)
                {
                    var props = new TMXProperties();
                    LoadProperties(reader, props);

                    map.SetProperties(props);
                }

                if (!LoadTMXSections(reader, ref map))
                {
                    Debug.Log("Failed to load TMX sections from " + fileName);
                    return null;
                }
            }
            catch (IOException e)
            {
                Debug.Log("Failed to load TMX from file " + fileName + ", " + e.ToString());
                return null;
            }

            return map;
        }

        /// <summary>
        /// Load a layer definition from an map definition
        /// </summary>
        /// <param name="tmx">Map definition</param>
        /// <param name="tmxSourceLayer">Name of the layer to load</param>
        /// <param name="destinationLayer">Destination RetroBlit layer</param>
        /// <param name="sourceRect">Source rectangle</param>
        /// <param name="destPos">Destination position</param>
        /// <param name="packedSpriteLookup">Lookup table for translating TMX tile indexes to packed sprites</param>
        /// <returns>True if successful</returns>
        public bool LoadTMXLayer(TMXMap tmx, string tmxSourceLayer, int destinationLayer, Rect2i sourceRect, Vector2i destPos, PackedSpriteID[] packedSpriteLookup)
        {
            TMXMapDef map = null;

            if (!(tmx is RetroBlitInternal.RetroBlitTilemap.TMXMapDef))
            {
                Debug.LogError("Can't load TMX layer, invalid map object!");
                return false;
            }

            map = (TMXMapDef)tmx;

            if (map == null || map.realPathName == null || map.realPathName.Length == 0 || map.layers == null)
            {
                Debug.LogError("Can't load TMX layer, invalid map, or map not open yet!");
                return false;
            }

            if (map.infinite)
            {
                Debug.LogError("TMX map is infinite, use MapLoadTMXLayerChunk() instead");
                return false;
            }

            if (!map.layers.ContainsKey(tmxSourceLayer))
            {
                Debug.LogError("Layer " + tmxSourceLayer + " not found");
                return false;
            }

            var tmxLayer = (TMXLayerDef)map.layers[tmxSourceLayer];

            var layerNameHash = mWorkStr.Set(tmxSourceLayer).ToLowerInvariant().GetHashCode().ToString("x");

            var tmxFileName = map.realPathName + "layer_" + layerNameHash;
            var tmxFile = Resources.Load<TextAsset>(tmxFileName);

            if (tmxFile == null)
            {
                Debug.LogError("Can't find TMX file when loading TMX layer!");
                return false;
            }

            var tmxBytes = tmxFile.bytes;

            var decompressed = RetroBlitDeflate.Decompress(tmxBytes, 0, tmxBytes.Length);
            if (decompressed == null || decompressed.Length <= 0)
            {
                Debug.LogError("Could not decompress tile data for layer " + tmxSourceLayer);
                return false;
            }

            var tileDataReader = new BinaryReader(new MemoryStream(decompressed));

            if (tileDataReader == null)
            {
                Debug.LogError("Could not read tile data for layer " + tmxSourceLayer);
                return false;
            }

            Color32 color = Color.white;

            int sx = 0;
            int sy = 0;

            int sx0 = sourceRect.x;
            int sx1 = sourceRect.x + sourceRect.width;

            int sy0 = sourceRect.y;
            int sy1 = sourceRect.y + sourceRect.height;

            int dx = destPos.x;
            int dy = destPos.y;

            while (tileDataReader.PeekChar() >= 0)
            {
                byte tsxIndex = tileDataReader.ReadByte();
                byte flags = tileDataReader.ReadByte();
                int tileId = tileDataReader.ReadInt32();

                if (packedSpriteLookup != null)
                {
                    if (tileId < packedSpriteLookup.Length && tileId >= 0)
                    {
                        tileId = packedSpriteLookup[tileId].id;
                        flags |= RetroBlitInternal.RetroBlitTilemap.SPRITEPACK;
                    }
                }

                if (sx >= sx0 && sx <= sx1 && sy >= sy0 && sy <= sy1)
                {
                    SpriteSet(destinationLayer, dx, dy, tileId, color, flags);

                    // Set properties if available
                    if (tsxIndex >= 0 && tsxIndex < map.allTileProperties.Count)
                    {
                        var props = map.allTileProperties[tsxIndex];
                        if (props != null)
                        {
                            if (props.ContainsKey(tileId))
                            {
                                DataSet<TMXProperties>(destinationLayer, dx, dy, props[tileId]);
                            }
                        }
                    }

                    dx++;
                }

                sx++;
                if (sx >= tmxLayer.size.x)
                {
                    sx = 0;
                    dx = destPos.x;
                    sy++;
                    dy++;
                }

                if (sy >= tmxLayer.size.y || sy >= sourceRect.y + sourceRect.height)
                {
                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// Load a single layer chunk
        /// </summary>
        /// <param name="tmx">Map definition</param>
        /// <param name="tmxSourceLayer">Name of source layer</param>
        /// <param name="destinationLayer">RetroBlit destination layer</param>
        /// <param name="chunkOffset">Chunk offset</param>
        /// <param name="destPos">Destination position</param>
        /// <param name="packedSpriteLookup">Lookup table for translating TMX tile indexes to packed sprites</param>
        /// <returns>True if successful</returns>
        public bool LoadTMXLayerChunk(TMXMap tmx, string tmxSourceLayer, int destinationLayer, Vector2i chunkOffset, Vector2i destPos, PackedSpriteID[] packedSpriteLookup)
        {
            TMXMapDef map = null;

            if (!(tmx is RetroBlitInternal.RetroBlitTilemap.TMXMapDef))
            {
                Debug.LogError("Can't load TMX layer, invalid map object!");
                return false;
            }

            map = (TMXMapDef)tmx;

            if (map == null || map.realPathName == null || map.realPathName.Length == 0 || map.layers == null)
            {
                Debug.LogError("Can't load TMX layer, invalid map, or map not open yet!");
                return false;
            }

            if (!map.infinite)
            {
                Debug.LogError("TMX map is not infinite, use LoadTMXLayer() instead");
                return false;
            }

            if (!map.layers.ContainsKey(tmxSourceLayer))
            {
                Debug.LogError("Layer " + tmxSourceLayer + " not found");
                return false;
            }

            int chunkWidth = map.chunkSize.x;
            int chunkHeight = map.chunkSize.y;

            var tmxLayer = (TMXLayerDef)map.layers[tmxSourceLayer];

            ulong part1 = (ulong)chunkOffset.x;
            ulong part2 = (ulong)chunkOffset.y;
            ulong offset = ((part1 << 32) & 0xFFFFFFFF00000000) | (part2 & 0xFFFFFFFF);

            int layerNameHash = mWorkStr.Set(tmxSourceLayer).ToLowerInvariant().GetHashCode();
            var tupleKey = new RetroBlitTuple<int, ulong>(layerNameHash, offset);

            var decompressed = map.mChunkLRU.Get(tupleKey);
            if (decompressed == null)
            {
                var chunkTable = GetLayerIndexTable(map, layerNameHash);

                if (chunkTable == null)
                {
                    Debug.LogError("TMX could not load chunk index table for layer " + tmxSourceLayer);
                    return false;
                }

                // If the chunk can't be found then fail silently and wipe the chunk area. This will also
                // release the chunk geometry on next draw because it will not have any vertices
                if (!chunkTable.ContainsKey(offset))
                {
                    for (int y = destPos.y; y < destPos.y + chunkHeight; y++)
                    {
                        for (int x = destPos.x; x < destPos.x + chunkWidth; x++)
                        {
                            mRetroBlitAPI.Tilemap.SpriteSet(destinationLayer, x, y, RB.SPRITE_EMPTY, Color.white, 0);

                            Tile[] tilesArr;
                            int tileIndex;
                            if (GetTileRef(destinationLayer, x, y, out tilesArr, out tileIndex, true))
                            {
                                tilesArr[tileIndex].data = null;
                            }
                        }
                    }

                    return true;
                }

                var chunkDef = chunkTable[offset];

                var chunkFileName = map.realPathName + "layer_" + layerNameHash.ToString("x") + "_seg_" + chunkDef.segmentIndex;

                var chunkFile = Resources.Load<TextAsset>(chunkFileName);

                if (chunkFile == null)
                {
                    Debug.LogError("Can't find TMX file when loading TMX layer!");
                    return false;
                }

                var chunkBytes = chunkFile.bytes;

                decompressed = RetroBlitDeflate.Decompress(chunkBytes, chunkDef.segmentOffset, chunkDef.compressedLength);
                if (decompressed == null || decompressed.Length <= 0)
                {
                    Debug.LogError("Could not decompress tile data for layer " + tmxSourceLayer);
                    return false;
                }

                map.mChunkLRU.Add(tupleKey, decompressed, decompressed.Length);
            }

            var tileDataReader = new BinaryReader(new MemoryStream(decompressed));

            if (tileDataReader == null)
            {
                Debug.LogError("Could not read tile data for layer " + tmxSourceLayer);
                return false;
            }

            Color32 color = Color.white;

            int sx = 0;
            int sy = 0;

            int dx = destPos.x;
            int dy = destPos.y;

            while (tileDataReader.PeekChar() >= 0)
            {
                // Skip tsxIndex, don't need it for now
                tileDataReader.ReadByte();

                byte flags = tileDataReader.ReadByte();
                int tileId = tileDataReader.ReadInt32();

                if (packedSpriteLookup != null)
                {
                    if (packedSpriteLookup != null)
                    {
                        if (tileId < packedSpriteLookup.Length && tileId >= 0)
                        {
                            tileId = packedSpriteLookup[tileId].id;
                            flags |= RetroBlitInternal.RetroBlitTilemap.SPRITEPACK;
                        }
                    }
                }

                SpriteSet(destinationLayer, dx, dy, tileId, color, flags);
                dx++;
                sx++;

                if (sx >= chunkWidth)
                {
                    sx = 0;
                    dx = destPos.x;
                    sy++;
                    dy++;
                }

                if (sy >= chunkHeight)
                {
                    break;
                }
            }

            return true;
        }

        private bool LoadTMXSections(BinaryReader reader, ref TMXMapDef map)
        {
            while (true)
            {
                int section = reader.ReadByte();
                if (section == RetroBlit_TMX_SECTION_TILE_LAYER)
                {
                    if (!LoadTMXTileLayerDef(reader, ref map))
                    {
                        return false;
                    }
                }
                else if (section == RetroBlit_TMX_SECTION_OBJECTGROUP)
                {
                    if (!LoadTMXObjectGroupDef(reader, ref map))
                    {
                        return false;
                    }
                }
                else if (section == RetroBlit_TSX_PROPERTIES)
                {
                    if (!LoadTSXProperties(reader, ref map))
                    {
                        return false;
                    }
                }
                else if (section == RetroBlit_TMX_SECTION_END)
                {
                    break;
                }
            }

            return true;
        }

        private bool LoadTMXTileLayerDef(BinaryReader reader, ref TMXMapDef map)
        {
            TMXLayerDef layerDef = new TMXLayerDef();
            string name = reader.ReadString();
            int layerWidth = reader.ReadInt32();
            int layerHeight = reader.ReadInt32();
            int layerOffsetX = reader.ReadInt32();
            int layerOffsetY = reader.ReadInt32();
            bool layerVisible = reader.ReadBoolean();
            byte layerAlpha = reader.ReadByte();
            int chunkCount = 0;

            // Load properties if available
            bool propsAvailable = reader.ReadBoolean();
            if (propsAvailable)
            {
                var props = new TMXProperties();
                LoadProperties(reader, props);

                layerDef.SetProperties(props);
            }

            if (map.infinite)
            {
                chunkCount = reader.ReadInt32();
            }

            layerDef.chunkCount = chunkCount;
            layerDef.SetSize(new Vector2i(layerWidth, layerHeight));
            layerDef.SetOffset(new Vector2i(layerOffsetX, layerOffsetY));
            layerDef.SetVisible(layerVisible);
            layerDef.SetAlpha(layerAlpha);

            map.layers[name] = layerDef;

            return true;
        }

        private bool LoadTMXObjectGroupDef(BinaryReader reader, ref TMXMapDef map)
        {
            var objectGroup = new TMXObjectGroupDef();

            var name = reader.ReadString();
            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var a = reader.ReadByte();
            var alpha = reader.ReadByte();
            var visible = reader.ReadBoolean();
            var offsetX = reader.ReadInt32();
            var offsetY = reader.ReadInt32();

            objectGroup.SetName(name);
            objectGroup.SetColor(new Color32(r, g, b, a));
            objectGroup.SetAlpha(alpha);
            objectGroup.SetVisible(visible);
            objectGroup.SetOffset(new Vector2i(offsetX, offsetY));

            // Load properties if available
            bool propsAvailable = reader.ReadBoolean();
            if (propsAvailable)
            {
                var props = new TMXProperties();
                LoadProperties(reader, props);

                objectGroup.SetProperties(props);
            }

            // Now load objects
            var objects = new List<TMXObject>();
            var objectCount = reader.ReadInt32();

            for (int i = 0; i < objectCount; i++)
            {
                var objName = reader.ReadString();
                var objType = reader.ReadString();
                var rectX = reader.ReadInt32();
                var rectY = reader.ReadInt32();
                var rectWidth = reader.ReadInt32();
                var rectHeight = reader.ReadInt32();
                var rotation = reader.ReadSingle();
                var objVisible = reader.ReadBoolean();
                var shape = reader.ReadInt32();

                var points = new List<Vector2i>();
                var pointsCount = reader.ReadInt32();
                for (int j = 0; j < pointsCount; j++)
                {
                    var pointX = reader.ReadInt32();
                    var pointY = reader.ReadInt32();
                    points.Add(new Vector2i(pointX, pointY));
                }

                var tmxObject = new TMXObjectDef();
                tmxObject.SetName(objName);
                tmxObject.SetType(objType);
                tmxObject.SetShape((TMXObject.Shape)shape);
                tmxObject.SetRect(new Rect2i(rectX, rectY, rectWidth, rectHeight));
                tmxObject.SetRotation(rotation);
                tmxObject.SetVisible(objVisible);
                tmxObject.SetPoints(points);

                // Load properties if available
                propsAvailable = reader.ReadBoolean();
                if (propsAvailable)
                {
                    var props = new TMXProperties();
                    LoadProperties(reader, props);

                    tmxObject.SetProperties(props);
                }

                objects.Add(tmxObject);
            }

            objectGroup.SetObjects(objects);

            map.objectGroups[name] = objectGroup;

            return true;
        }

        private void LoadProperties(BinaryReader reader, TMXProperties props)
        {
            var strCount = reader.ReadInt32();
            for (int i = 0; i < strCount; i++)
            {
                var key = reader.ReadString();
                var val = reader.ReadString();
                props.Add(key, val);
            }

            var boolCount = reader.ReadInt32();
            for (int i = 0; i < boolCount; i++)
            {
                var key = reader.ReadString();
                bool val = reader.ReadBoolean();
                props.Add(key, val);
            }

            var intCount = reader.ReadInt32();
            for (int i = 0; i < intCount; i++)
            {
                var key = reader.ReadString();
                int val = reader.ReadInt32();
                props.Add(key, val);
            }

            var floatCount = reader.ReadInt32();
            for (int i = 0; i < floatCount; i++)
            {
                var key = reader.ReadString();
                float val = reader.ReadSingle();
                props.Add(key, val);
            }

            var colorCount = reader.ReadInt32();
            for (int i = 0; i < colorCount; i++)
            {
                var key = reader.ReadString();
                Color32 val = new Color32();

                val.r = reader.ReadByte();
                val.g = reader.ReadByte();
                val.b = reader.ReadByte();
                val.a = reader.ReadByte();

                props.Add(key, val);
            }
        }

        private bool LoadTSXProperties(BinaryReader reader, ref TMXMapDef map)
        {
            int tsxLoops = 0;

            var allProps = new List<Dictionary<int, TMXProperties>>();

            int prevTsxIndex = -1;

            while (true)
            {
                var tsxIndex = reader.ReadInt32();
                if (tsxIndex == -1)
                {
                    // All done
                    break;
                }

                if (tsxIndex != prevTsxIndex + 1)
                {
                    Debug.LogError("TMX binary files had non-consequitive TSX property sets, TMX import is likely corrupt, please try reimporting " + map.realPathName);
                    return false;
                }

                var propsSet = new Dictionary<int, TMXProperties>();

                while (true)
                {
                    var tid = reader.ReadInt32();
                    if (tid == -1)
                    {
                        // All done this tsx
                        break;
                    }

                    var props = new TMXProperties();

                    LoadProperties(reader, props);

                    propsSet.Add(tid, props);
                }

                allProps.Add(propsSet);

                prevTsxIndex = tsxIndex;

                // Break out if we loop for too long, TSX data could be corrupt
                tsxLoops++;
                if (tsxLoops > 256)
                {
                    Debug.Log("TSX properties data is invalid, please try to reimport " + map.realPathName);
                    return false;
                }
            }

            map.allTileProperties = allProps;

            return true;
        }

        private Dictionary<ulong, ChunkDef> GetLayerIndexTable(TMXMapDef tmx, int layerName)
        {
            if (tmx == null)
            {
                return null;
            }

            var fileName = tmx.realPathName + "layer_" + layerName.ToString("x") + "_index";

            var cached = tmx.mLayerIndexLRU.Get(fileName);
            if (cached != null)
            {
                return cached;
            }

            var indexTableFile = Resources.Load<TextAsset>(fileName);

            if (indexTableFile == null)
            {
                Debug.Log("TMX could not find layer index table");
                return null;
            }

            try
            {
                var reader = new BinaryReader(new MemoryStream(indexTableFile.bytes));

                int byteSize = 0;

                int chunkCount = reader.ReadInt32();
                byteSize += 4;

                var table = new Dictionary<ulong, ChunkDef>();

                // Return empty table if there are no chunks
                if (chunkCount == 0)
                {
                    return table;
                }

                for (int i = 0; i < chunkCount; i++)
                {
                    var chunkDef = new ChunkDef();

                    ulong offset = reader.ReadUInt64();
                    byteSize += 8;
                    chunkDef.segmentIndex = reader.ReadUInt16();
                    byteSize += 2;
                    chunkDef.segmentOffset = reader.ReadUInt16();
                    byteSize += 2;
                    chunkDef.compressedLength = reader.ReadUInt16();
                    byteSize += 2;

                    table[offset] = chunkDef;
                }

                tmx.mLayerIndexLRU.Add(fileName, table, byteSize);

                return table;
            }
            catch (IOException e)
            {
                Debug.Log("Failed to load layer index from file " + fileName + ", " + e.ToString());
                return null;
            }
        }

        private struct ChunkDef
        {
            public ushort segmentIndex;
            public ushort segmentOffset;
            public ushort compressedLength;
        }

        /// <summary>
        /// Simple tuple class implementation.
        /// </summary>
        /// <typeparam name="FT">First type</typeparam>
        /// <typeparam name="ST">Second type</typeparam>
        public class RetroBlitTuple<FT, ST> : IEquatable<RetroBlitTuple<FT, ST>>
        {
            /// <summary>
            /// First type
            /// </summary>
            public FT First;

            /// <summary>
            /// Second type
            /// </summary>
            public ST Second;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="first">First value</param>
            /// <param name="second">Second value</param>
            public RetroBlitTuple(FT first, ST second)
            {
                First = first;
                Second = second;
            }

            /// <summary>
            /// Combined hash code of the values
            /// </summary>
            /// <returns>Hashcode</returns>
            public override int GetHashCode()
            {
                int hash = 17;
                hash = (hash * 31) + First.GetHashCode();
                hash = (hash * 31) + Second.GetHashCode();
                return hash;
            }

            /// <summary>
            /// Equality check
            /// </summary>
            /// <param name="tuple">Other tuple</param>
            /// <returns>True if equal</returns>
            public bool Equals(RetroBlitTuple<FT, ST> tuple)
            {
                return tuple.First.Equals(First) && tuple.Second.Equals(Second);
            }

            /// <summary>
            /// Equality check
            /// </summary>
            /// <param name="o">Other object</param>
            /// <returns>True if equal</returns>
            public override bool Equals(object o)
            {
                return this.Equals(o as RetroBlitTuple<FT, ST>);
            }
        }

        private class TMXLayerDef : TMXLayer
        {
            /// <summary>
            /// Count of chunks in this layer, if the map is infinite
            /// </summary>
            public int chunkCount;

            /// <summary>
            /// Set the size of layer
            /// </summary>
            /// <param name="size">Size</param>
            public void SetSize(Vector2i size)
            {
                mSize = size;
            }

            /// <summary>
            /// Set the offset of layer
            /// </summary>
            /// <param name="offset">Offset</param>
            public void SetOffset(Vector2i offset)
            {
                mOffset = offset;
            }

            /// <summary>
            /// Set the visible flag of layer
            /// </summary>
            /// <param name="visible">Visible flag</param>
            public void SetVisible(bool visible)
            {
                mVisible = visible;
            }

            /// <summary>
            /// Set the alpha transparency of layer
            /// </summary>
            /// <param name="alpha">Alpha</param>
            public void SetAlpha(byte alpha)
            {
                mAlpha = alpha;
            }

            /// <summary>
            /// Set the custom properties of the layer
            /// </summary>
            /// <param name="props">Properties</param>
            public void SetProperties(TMXProperties props)
            {
                mProperties = props;
            }
        }

        private class TMXObjectGroupDef : TMXObjectGroup
        {
            /// <summary>
            /// Set the name
            /// </summary>
            /// <param name="name">Name</param>
            public void SetName(string name)
            {
                mName = name;
            }

            /// <summary>
            /// Set color
            /// </summary>
            /// <param name="color">Color</param>
            public void SetColor(Color32 color)
            {
                mColor = color;
            }

            /// <summary>
            /// Set alpha transparency
            /// </summary>
            /// <param name="alpha">Alpha transparency</param>
            public void SetAlpha(byte alpha)
            {
                mAlpha = alpha;
            }

            /// <summary>
            /// Set visible flag
            /// </summary>
            /// <param name="visible">Visible flag</param>
            public void SetVisible(bool visible)
            {
                mVisible = visible;
            }

            /// <summary>
            /// Set offset
            /// </summary>
            /// <param name="offset">Offset</param>
            public void SetOffset(Vector2i offset)
            {
                mOffset = offset;
            }

            /// <summary>
            /// Set objects list
            /// </summary>
            /// <param name="objects">Objects list</param>
            public void SetObjects(List<TMXObject> objects)
            {
                mObjects = objects;
            }

            /// <summary>
            /// Set custom properties
            /// </summary>
            /// <param name="props">Custom properties</param>
            public void SetProperties(TMXProperties props)
            {
                mProperties = props;
            }
        }

        private class TMXMapDef : TMXMap
        {
            /// <summary>
            /// Actual file path of the map
            /// </summary>
            public string realPathName;

            /// <summary>
            /// Size of chunks in this map
            /// </summary>
            public Vector2i chunkSize;

            /// <summary>
            /// All tile properties for the map. Properties are stored in a list per tileset.
            /// </summary>
            public List<Dictionary<int, TMXProperties>> allTileProperties = new List<Dictionary<int, TMXProperties>>();

            /// <summary>
            /// LRU cache of layer index tables, null for non-infinite maps.
            /// </summary>
            public RetroBlitLRUCache<string, Dictionary<ulong, ChunkDef>> mLayerIndexLRU = null;

            /// <summary>
            /// LRU cache of chunks, null for non-infinite maps.
            /// </summary>
            public RetroBlitLRUCache<RetroBlitTuple<int, ulong>, byte[]> mChunkLRU = null;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="fileName">Filename</param>
            public TMXMapDef(string fileName)
            {
                mFileName = fileName;
            }

            /// <summary>
            /// Set size
            /// </summary>
            /// <param name="size">Size</param>
            public void SetSize(Vector2i size)
            {
                mSize = size;
            }

            /// <summary>
            /// Set infinite flag, if true then also allocate the LRUs
            /// </summary>
            /// <param name="infinite">Infinite flag</param>
            public void SetInfinite(bool infinite)
            {
                mInfinite = infinite;
                if (mInfinite && (mLayerIndexLRU == null || mChunkLRU == null))
                {
                    mLayerIndexLRU = new RetroBlitLRUCache<string, Dictionary<ulong, ChunkDef>>(1024 * 512); // 512k cache
                    mChunkLRU = new RetroBlitLRUCache<RetroBlitTuple<int, ulong>, byte[]>(1024 * 1024 * 2); // 2M cache
                }
            }

            /// <summary>
            /// Set background color
            /// </summary>
            /// <param name="color">Color</param>
            public void SetBackgroundColor(Color32 color)
            {
                mBackgroundColor = color;
            }

            /// <summary>
            /// Set custom properties
            /// </summary>
            /// <param name="props">Properties</param>
            public void SetProperties(TMXProperties props)
            {
                mProperties = props;
            }
        }

        private class TMXObjectDef : TMXObject
        {
            /// <summary>
            /// Set name of object
            /// </summary>
            /// <param name="name">Name</param>
            public void SetName(string name)
            {
                mName = name;
            }

            /// <summary>
            /// Set type of object
            /// </summary>
            /// <param name="type">Type</param>
            public void SetType(string type)
            {
                mType = type;
            }

            /// <summary>
            /// Set shape of object
            /// </summary>
            /// <param name="shape">Shape</param>
            public void SetShape(Shape shape)
            {
                mShape = shape;
            }

            /// <summary>
            /// Set rect of object
            /// </summary>
            /// <param name="rect">Rect</param>
            public void SetRect(Rect2i rect)
            {
                mRect = rect;
            }

            /// <summary>
            /// Set rotation of object
            /// </summary>
            /// <param name="rotation">Rotation</param>
            public void SetRotation(float rotation)
            {
                mRotation = rotation;
            }

            /// <summary>
            /// Set visible flag of object
            /// </summary>
            /// <param name="visible">Visible flag</param>
            public void SetVisible(bool visible)
            {
                mVisible = visible;
            }

            /// <summary>
            /// Set points of object
            /// </summary>
            /// <param name="points">Points list</param>
            public void SetPoints(List<Vector2i> points)
            {
                mPoints = points;
            }

            /// <summary>
            /// Set custom properties of the object
            /// </summary>
            /// <param name="props">Properties</param>
            public void SetProperties(TMXProperties props)
            {
                mProperties = props;
            }
        }
    }
}
