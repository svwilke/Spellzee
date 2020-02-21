using System.Collections.Generic;
using UnityEngine;

/*********************************************************************************
* The comments in this file are used to generate the API documentation. Please see
* Assets/RetroBlit/Docs for much easier reading!
*********************************************************************************/

/// <summary>
/// A definition of a TMX Tilemap
/// </summary>
/// <remarks>
/// A definition of a TMX Tilemap returned by <see cref="RB.MapLoadTMX"/>. This definition contains various information about the map, its layers, and objects.
/// It does not however contain layer tile data, tile data is loaded with <see cref="RB.MapLoadTMXLayer"/>, and <see cref="RB.MapLoadTMXLayerChunk"/>. Because
/// the tile data is loaded and stored separately the <see cref="TMXMap"/> is relatively light-weight and can be kept in memory for future reference.
/// </remarks>
public class TMXMap
{
    /// <summary>
    /// File name of the TMX tilemap
    /// </summary>
    protected string mFileName;

    /// <summary>
    /// Size of the tilemap in tiles
    /// </summary>
    protected Vector2i mSize;

    /// <summary>
    /// Infinite flag
    /// </summary>
    protected bool mInfinite;

    /// <summary>
    /// Background color of the tilemap
    /// </summary>
    protected Color32 mBackgroundColor;

    /// <summary>
    /// Custom properties of the tilemap
    /// </summary>
    protected TMXProperties mProperties = new TMXProperties();

    /// <summary>
    /// Layers in the map
    /// </summary>
    private Dictionary<string, TMXLayer> mLayers = new Dictionary<string, TMXLayer>();

    /// <summary>
    /// Object groups in the map
    /// </summary>
    private Dictionary<string, TMXObjectGroup> mObjectGroups = new Dictionary<string, TMXObjectGroup>();

    /// <summary>
    /// Infinite map flag.
    /// </summary>
    /// <remarks>
    /// Infinite map flag. This flag is set if the map is composed of chunks rather than a single block. The maximum tile coordinates of such a map range from -2147483648 to 2147483648,
    /// which in practical terms makes the map infinite.
    ///
    /// Tiles for non-infinite maps are loaded with <see cref="RB.MapLoadTMXLayer"/>, for infinite maps they are loaded with <see cref="RB.MapLoadTMXLayerChunk"/>.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public bool infinite
    {
        get { return mInfinite; }
    }

    /// <summary>
    /// Size of the map in terms of tile count.
    /// </summary>
    /// <remarks>
    /// Size of the map in terms of tile count. If map is infinite this size is calculated from the minimum and maximum offsets between all chunks in the map.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public Vector2i size
    {
        get { return mSize; }
    }

    /// <summary>
    /// Background color of the map
    /// </summary>
    /// <remarks>
    /// Background color of the map as defined in the Tiled editor.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    public Color32 backgroundColor
    {
        get { return mBackgroundColor; }
    }

    /// <summary>
    /// All layers in the map, keyed by their layer name.
    /// </summary>
    /// <remarks>
    /// All layers in the map, keyed by their layer name.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXLayer"/>
    public Dictionary<string, TMXLayer> layers
    {
        get { return mLayers; }
    }

    /// <summary>
    /// All object groups of the map, keyed by their names.
    /// </summary>
    /// <remarks>
    /// All object groups of the map, keyed by their names.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXObjectGroup"/>
    public Dictionary<string, TMXObjectGroup> objectGroups
    {
        get { return mObjectGroups; }
    }

    /// <summary>
    /// Custom properties of the map
    /// </summary>
    /// <remarks>
    /// Collection of all custom properties defined for the map.
    /// <seedoc>Features:Tiled TMX Support</seedoc>
    /// </remarks>
    /// <seealso cref="TMXProperties"/>
    public TMXProperties properties
    {
        get { return mProperties; }
    }
}
