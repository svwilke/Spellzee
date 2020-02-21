#define PIXEL_TEST

namespace RetroBlitInternal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Renderer subsystem
    /// </summary>
    public sealed class RetroBlitRenderer
    {
        /// <summary>
        /// Magic number to quickly verify that this is a valid RetroBlit file
        /// </summary>
        public const ushort RetroBlit_SP_MAGIC = 0x05EF;

        /// <summary>
        /// Version
        /// </summary>
        public const ushort RetroBlit_SP_VERSION = 0x0001;

        /// <summary>
        /// Current sprite sheet
        /// </summary>
        public SpriteSheet CurrentSpriteSheet;

        /// <summary>
        /// Current sprite sheet index. DUMMY_SPRITESHEET_INDEX is a dummy value that indicates no sprite sheet set. Using dummy saves us from having to do a conditional statement
        /// </summary>
        public int CurrentSpriteSheetIndex = DUMMY_SPRITESHEET_INDEX;

        /// <summary>
        /// All sprite sheets
        /// </summary>
        public SpriteSheet[] SpriteSheets = new SpriteSheet[RetroBlitHW.HW_MAX_SPRITESHEETS + 1];

        /// <summary>
        /// System texture
        /// </summary>
        public Texture2D SystemTexture;

        /// <summary>
        /// Is rendering enabled? Rendering is enabled during <see cref="RB.IRetroBlitGame.Render"/> call
        /// </summary>
        public bool RenderEnabled = false;

        private const int MAX_QUADS_PER_MESH = 512;
        private const int MAX_INDICES_PER_MESH = 6 * MAX_QUADS_PER_MESH;
        private const int MAX_VERTEX_PER_MESH = 4 * MAX_QUADS_PER_MESH;

        private const int MAX_ELLIPSE_RADIUS = RetroBlitHW.HW_MAX_POLY_POINTS / 2;

        private const int DUMMY_SPRITESHEET_INDEX = RetroBlitHW.HW_MAX_SPRITESHEETS;

        private FrontBuffer mFrontBuffer = new FrontBuffer();
        private RenderTexture mCurrentRenderTexture = null;

        private MeshStorage mMeshStorage;

        private Material mCurrentDrawMaterial;
        private Material mDrawMaterialRGB;
        private Material mDrawMaterialClear;

        private SpriteSheet mEmptySpriteSheet = new SpriteSheet();
        private int mCurrentBatchSpriteIndex = -1;

        private Texture mPreviousTexture;

        private RetroBlitShader[] mShaders = new RetroBlitShader[RetroBlitHW.HW_MAX_SHADERS];
        private int mCurrentShaderIndex = -1;

        private ClipRegion mClipRegion;
        private Rect2i mClip;
        private bool mClipDebug;
        private Color32 mClipDebugColor;
        private List<DebugClipRegion> mDebugClipRegions = new List<DebugClipRegion>();

        private Vector2i mCameraPos;

        private RetroBlitAPI mRetroBlitAPI = null;

        private Color32 mCurrentColor = new Color32(255, 255, 255, 255);

        private Vector2i[] mPoints = new Vector2i[RetroBlitHW.HW_MAX_POLY_POINTS];

        private bool mShowFlushDebug = false;
        private Color32 mFlushDebugFontColor = Color.white;
        private Color32 mFlushDebugBackgroundColor = Color.black;

        private int mPropIDGlobalTint;
        private int mPropIDSpritesTexture;
        private int mPropIDClip;
        private int mPropIDDisplaySize;

        private FlushInfo[] mFlushInfo = new FlushInfo[]
        {
            new FlushInfo() { Reason = "Batch Full", Count = 0 },
            new FlushInfo() { Reason = "Spritesheet Change", Count = 0 },
            new FlushInfo() { Reason = "Tilemap Chunk", Count = 0 },
            new FlushInfo() { Reason = "Frame End", Count = 0 },
            new FlushInfo() { Reason = "Clip Change", Count = 0 },
            new FlushInfo() { Reason = "Offscreen Change", Count = 0 },
            new FlushInfo() { Reason = "Present/Effect Apply", Count = 0 },
            new FlushInfo() { Reason = "Shader Apply", Count = 0 },
            new FlushInfo() { Reason = "Shader Reset", Count = 0 },
            new FlushInfo() { Reason = "Set Material", Count = 0 },
            new FlushInfo() { Reason = "Set Texture", Count = 0 },
        };

        /// <summary>
        /// Reasons for flushing to Mesh, each flush generates a Unity batch draw call
        /// </summary>
        public enum FlushReason
        {
            /// <summary>
            /// Flushed because batch is full
            /// </summary>
            BATCH_FULL,

            /// <summary>
            /// Flushed because spritesheet changed
            /// </summary>
            SPRITESHEET_CHANGE,

            /// <summary>
            /// Flushed because tilemap chunk was drawn
            /// </summary>
            TILEMAP_CHUNK,

            /// <summary>
            /// Flushed because frame ended
            /// </summary>
            FRAME_END,

            /// <summary>
            /// Flushed because clip region changed
            /// </summary>
            CLIP_CHANGE,

            /// <summary>
            /// Flushed because offscreen SpriteSheet target changed
            /// </summary>
            OFFSCREEN_CHANGE,

            /// <summary>
            /// Flushed because of an effect was applied
            /// </summary>
            EFFECT_APPLY,

            /// <summary>
            /// Flushed because shader changed
            /// </summary>
            SHADER_APPLY,

            /// <summary>
            /// Flushed because shader was reset
            /// </summary>
            SHADER_RESET,

            /// <summary>
            /// Flush because material changed
            /// </summary>
            SET_MATERIAL,

            /// <summary>
            /// Flushed because texture changed
            /// </summary>
            SET_TEXTURE,
        }

        /// <summary>
        /// Initialize the subsystem
        /// </summary>
        /// <param name="api">Subsystem wrapper reference</param>
        /// <returns>True if successful</returns>
        public bool Initialize(RetroBlitAPI api)
        {
            mPropIDGlobalTint = Shader.PropertyToID("_GlobalTint");
            mPropIDSpritesTexture = Shader.PropertyToID("_SpritesTexture");
            mPropIDClip = Shader.PropertyToID("_Clip");
            mPropIDDisplaySize = Shader.PropertyToID("_DisplaySize");

            if (api == null)
            {
                return false;
            }

            // Setup a dummy spritesheet, it acts like not having a spritesheet at all, saves us from doing conditional checks
            SpriteSheets[DUMMY_SPRITESHEET_INDEX].columns = 1;
            SpriteSheets[DUMMY_SPRITESHEET_INDEX].rows = 1;
            SpriteSheets[DUMMY_SPRITESHEET_INDEX].spriteSize = new Vector2i(1, 1);
            SpriteSheets[DUMMY_SPRITESHEET_INDEX].texture = new RenderTexture(1, 1, 1);
            SpriteSheets[DUMMY_SPRITESHEET_INDEX].textureSize = new Vector2i(1, 1);
            SpriteSheets[DUMMY_SPRITESHEET_INDEX].needsClear = false;

            // Set the current spritesheet to null so it's referencing nothing incase the user tries
            // to call Renderer apis in Update() when they should not!
            SetCurrentSpriteSheetEmpty();

            mRetroBlitAPI = api;

            mDrawMaterialRGB = mRetroBlitAPI.ResourceBucket.LoadMaterial("DrawMaterialRGB");
            if (mDrawMaterialRGB == null)
            {
                return false;
            }

            mDrawMaterialClear = mRetroBlitAPI.ResourceBucket.LoadMaterial("DrawMaterialClear");
            if (mDrawMaterialClear == null)
            {
                return false;
            }

            mMeshStorage = new MeshStorage();

            SetCurrentMaterial(mDrawMaterialRGB);

            if (!GenerateSystemTexture())
            {
                return false;
            }

            if (CurrentSpriteSheetIndex != DUMMY_SPRITESHEET_INDEX)
            {
                SetCurrentTexture(SpriteSheets[CurrentSpriteSheetIndex].texture, SpriteSheets[CurrentSpriteSheetIndex], true);
            }
            else
            {
                SetCurrentTexture(null, mEmptySpriteSheet, true);
            }

            return DisplayModeSet(RB.DisplaySize, mRetroBlitAPI.HW.PixelStyle);
        }

        /// <summary>
        /// Set display mode to given resolution and pixel style. Note that this sets only the RetroBlit pixel resolution, and does not affect the native
        /// window size. To change the native window size you can use the Unity Screen.SetResolution() API.
        /// </summary>
        /// <param name="resolution">Resolution</param>
        /// <param name="pixelStyle">Pixel style</param>
        /// <returns>True if mode was successfully set, false otherwise</returns>
        public bool DisplayModeSet(Vector2i resolution, RB.PixelStyle pixelStyle)
        {
            // Create main render target, and offscreen
            if (!mFrontBuffer.Resize(resolution, mRetroBlitAPI))
            {
                return false;
            }

            Onscreen();

            mRetroBlitAPI.HW.DisplaySize = resolution;
            mRetroBlitAPI.HW.PixelStyle = pixelStyle;

            return true;
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        /// <param name="color">RGB color</param>
        public void Clear(Color32 color)
        {
            if (!RenderEnabled)
            {
                return;
            }

            RenderTexture rt = UnityEngine.RenderTexture.active;
            UnityEngine.RenderTexture.active = mCurrentRenderTexture;
            GL.Clear(true, true, color);
            UnityEngine.RenderTexture.active = rt;

            // Drop whatever we may have been rendering before
            ResetMesh();
        }

        /// <summary>
        /// Clear a region of the render target. Useful for clearing spritesheets to alpha 0.
        /// </summary>
        /// <param name="color">RGB color</param>
        /// <param name="rect">Region to clear</param>
        public void ClearRect(Color32 color, Rect2i rect)
        {
            var prevMaterial = mCurrentDrawMaterial;
            SetCurrentMaterial(mDrawMaterialClear);

            DrawRectFill(rect, color, Vector2i.zero);

            SetCurrentMaterial(prevMaterial);
        }

        /// <summary>
        /// Draw a texture at given position
        /// </summary>
        /// <param name="srcRect">Source rectangle</param>
        /// <param name="destRect">Destination rectangle</param>
        public void DrawTexture(Rect2i srcRect, Rect2i destRect)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            int sx0 = srcRect.x;
            int sy0 = srcRect.y;
            int sx1 = sx0 + srcRect.width;
            int sy1 = sy0 + srcRect.height;

            int dx0 = 0;
            int dy0 = 0;
            int dx1;
            int dy1;

            dx1 = dx0 + destRect.width;
            dy1 = dy0 + destRect.height;

            float ux0, uy0, ux1, uy1;

            var spriteSheetTextureSize = SpriteSheets[CurrentSpriteSheetIndex].textureSize;

            ux0 = sx0 / (float)spriteSheetTextureSize.x;
            uy0 = 1.0f - (sy0 / ((float)spriteSheetTextureSize.y));
            ux1 = sx1 / (float)spriteSheetTextureSize.x;
            uy1 = 1.0f - (sy1 / ((float)spriteSheetTextureSize.y));

            Color32 color = mCurrentColor;

            dx0 -= mCameraPos.x - destRect.x;
            dy0 -= mCameraPos.y - destRect.y;

            dx1 -= mCameraPos.x - destRect.x;
            dy1 -= mCameraPos.y - destRect.y;

            // Early clip test
            if (dx1 < mClipRegion.x0 || dy1 < mClipRegion.y0 || dx0 > mClipRegion.x1 || dy0 > mClipRegion.y1)
            {
                return;
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            mMeshStorage.Verticies[i + 0].x = dx0;
            mMeshStorage.Verticies[i + 0].y = dy0;
            mMeshStorage.Verticies[i + 0].z = 1;

            mMeshStorage.Verticies[i + 1].x = dx1;
            mMeshStorage.Verticies[i + 1].y = dy0;
            mMeshStorage.Verticies[i + 1].z = 1;

            mMeshStorage.Verticies[i + 2].x = dx1;
            mMeshStorage.Verticies[i + 2].y = dy1;
            mMeshStorage.Verticies[i + 2].z = 1;

            mMeshStorage.Verticies[i + 3].x = dx0;
            mMeshStorage.Verticies[i + 3].y = dy1;
            mMeshStorage.Verticies[i + 3].z = 1;

            mMeshStorage.Uvs[i + 0].x = ux0;
            mMeshStorage.Uvs[i + 0].y = uy0;
            mMeshStorage.Uvs[i + 1].x = ux1;
            mMeshStorage.Uvs[i + 1].y = uy0;
            mMeshStorage.Uvs[i + 2].x = ux1;
            mMeshStorage.Uvs[i + 2].y = uy1;
            mMeshStorage.Uvs[i + 3].x = ux0;
            mMeshStorage.Uvs[i + 3].y = uy1;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;
            mMeshStorage.Colors[i + 3] = color;

            i += 4;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;
            mMeshStorage.Indices[j++] = i - 4;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw a texture at given position
        /// </summary>
        /// <param name="srcRect">Source rectangle</param>
        /// <param name="destRect">Destination rectangle</param>
        /// <param name="flags">Flags</param>
        public void DrawTexture(Rect2i srcRect, Rect2i destRect, int flags)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            int sx0 = srcRect.x;
            int sy0 = srcRect.y;
            int sx1 = sx0 + srcRect.width;
            int sy1 = sy0 + srcRect.height;

            int dx0 = 0;
            int dy0 = 0;
            int dx1;
            int dy1;

            if ((flags & RB.ROT_90_CW) == 0)
            {
                dx1 = dx0 + destRect.width;
                dy1 = dy0 + destRect.height;
            }
            else
            {
                dx1 = dx0 + destRect.height;
                dy1 = dy0 + destRect.width;
            }

            float ux0, uy0, ux1, uy1;
            float ux0raw, uy0raw, ux1raw, uy1raw;

            var spriteSheetTextureSize = SpriteSheets[CurrentSpriteSheetIndex].textureSize;

            ux0raw = sx0 / (float)spriteSheetTextureSize.x;
            uy0raw = 1.0f - (sy0 / ((float)spriteSheetTextureSize.y));
            ux1raw = sx1 / (float)spriteSheetTextureSize.x;
            uy1raw = 1.0f - (sy1 / ((float)spriteSheetTextureSize.y));

            if ((flags & RB.FLIP_H) == 0)
            {
                ux0 = ux0raw;
                ux1 = ux1raw;
            }
            else
            {
                ux0 = ux1raw;
                ux1 = ux0raw;
            }

            if ((flags & RB.FLIP_V) == 0)
            {
                uy0 = uy0raw;
                uy1 = uy1raw;
            }
            else
            {
                uy0 = uy1raw;
                uy1 = uy0raw;
            }

            Color32 color = mCurrentColor;

            dx0 -= mCameraPos.x - destRect.x;
            dy0 -= mCameraPos.y - destRect.y;

            dx1 -= mCameraPos.x - destRect.x;
            dy1 -= mCameraPos.y - destRect.y;

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            // Early clip test
            if (dx1 < mClipRegion.x0 || dy1 < mClipRegion.y0 || dx0 > mClipRegion.x1 || dy0 > mClipRegion.y1)
            {
                return;
            }

            if ((flags & RB.ROT_90_CW) == 0)
            {
                mMeshStorage.Verticies[i + 0].x = dx0;
                mMeshStorage.Verticies[i + 0].y = dy0;
                mMeshStorage.Verticies[i + 0].z = 1;

                mMeshStorage.Verticies[i + 1].x = dx1;
                mMeshStorage.Verticies[i + 1].y = dy0;
                mMeshStorage.Verticies[i + 1].z = 1;

                mMeshStorage.Verticies[i + 2].x = dx1;
                mMeshStorage.Verticies[i + 2].y = dy1;
                mMeshStorage.Verticies[i + 2].z = 1;

                mMeshStorage.Verticies[i + 3].x = dx0;
                mMeshStorage.Verticies[i + 3].y = dy1;
                mMeshStorage.Verticies[i + 3].z = 1;
            }
            else
            {
                mMeshStorage.Verticies[i + 0].x = dx1;
                mMeshStorage.Verticies[i + 0].y = dy0;
                mMeshStorage.Verticies[i + 0].z = 1;

                mMeshStorage.Verticies[i + 1].x = dx1;
                mMeshStorage.Verticies[i + 1].y = dy1;
                mMeshStorage.Verticies[i + 1].z = 1;

                mMeshStorage.Verticies[i + 2].x = dx0;
                mMeshStorage.Verticies[i + 2].y = dy1;
                mMeshStorage.Verticies[i + 2].z = 1;

                mMeshStorage.Verticies[i + 3].x = dx0;
                mMeshStorage.Verticies[i + 3].y = dy0;
                mMeshStorage.Verticies[i + 3].z = 1;
            }

            mMeshStorage.Uvs[i + 0].x = ux0;
            mMeshStorage.Uvs[i + 0].y = uy0;
            mMeshStorage.Uvs[i + 1].x = ux1;
            mMeshStorage.Uvs[i + 1].y = uy0;
            mMeshStorage.Uvs[i + 2].x = ux1;
            mMeshStorage.Uvs[i + 2].y = uy1;
            mMeshStorage.Uvs[i + 3].x = ux0;
            mMeshStorage.Uvs[i + 3].y = uy1;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;
            mMeshStorage.Colors[i + 3] = color;

            i += 4;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;
            mMeshStorage.Indices[j++] = i - 4;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw a texture at given position, rotation
        /// </summary>
        /// <param name="srcRect">Source rectangle</param>
        /// <param name="destRect">Destination rectangle</param>
        /// <param name="pivot">Rotation pivot point</param>
        /// <param name="rotation">Rotation in degrees</param>
        public void DrawTexture(Rect2i srcRect, Rect2i destRect, Vector2i pivot, float rotation)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            int sx0 = srcRect.x;
            int sy0 = srcRect.y;
            int sx1 = sx0 + srcRect.width;
            int sy1 = sy0 + srcRect.height;

            int dx0 = 0;
            int dy0 = 0;
            int dx1;
            int dy1;

            dx1 = dx0 + destRect.width;
            dy1 = dy0 + destRect.height;

            // Wrap the angle first to values between 0 and 360
            rotation = RetroBlitUtil.WrapAngle(rotation);

            float ux0, uy0, ux1, uy1;
            float ux0raw, uy0raw, ux1raw, uy1raw;

            var spriteSheetTextureSize = SpriteSheets[CurrentSpriteSheetIndex].textureSize;

            ux0raw = sx0 / (float)spriteSheetTextureSize.x;
            uy0raw = 1.0f - (sy0 / ((float)spriteSheetTextureSize.y));
            ux1raw = sx1 / (float)spriteSheetTextureSize.x;
            uy1raw = 1.0f - (sy1 / ((float)spriteSheetTextureSize.y));

            ux0 = ux0raw;
            ux1 = ux1raw;

            uy0 = uy0raw;
            uy1 = uy1raw;

            Color32 color = mCurrentColor;

            Vector3 p1, p2, p3, p4;

            p1 = new Vector3(dx0, dy0, 0);
            p2 = new Vector3(dx1, dy0, 0);
            p3 = new Vector3(dx1, dy1, 0);
            p4 = new Vector3(dx0, dy1, 0);

            var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotation), Vector3.one);
            matrix *= Matrix4x4.TRS(new Vector3(-pivot.x, -pivot.y, 0), Quaternion.identity, Vector3.one);

            p1 = matrix.MultiplyPoint3x4(p1);
            p2 = matrix.MultiplyPoint3x4(p2);
            p3 = matrix.MultiplyPoint3x4(p3);
            p4 = matrix.MultiplyPoint3x4(p4);

            p1.x += pivot.x;
            p1.y += pivot.y;

            p2.x += pivot.x;
            p2.y += pivot.y;

            p3.x += pivot.x;
            p3.y += pivot.y;

            p4.x += pivot.x;
            p4.y += pivot.y;

            p1.x -= mCameraPos.x - destRect.x;
            p1.y -= mCameraPos.y - destRect.y;

            p2.x -= mCameraPos.x - destRect.x;
            p2.y -= mCameraPos.y - destRect.y;

            p3.x -= mCameraPos.x - destRect.x;
            p3.y -= mCameraPos.y - destRect.y;

            p4.x -= mCameraPos.x - destRect.x;
            p4.y -= mCameraPos.y - destRect.y;

            // Early clip test
            if (p1.x < mClipRegion.x0 && p2.x < mClipRegion.x0 && p3.x < mClipRegion.x0 && p4.x < mClipRegion.x0)
            {
                return;
            }
            else if (p1.x > mClipRegion.x1 && p2.x > mClipRegion.x1 && p3.x > mClipRegion.x1 && p4.x > mClipRegion.x1)
            {
                return;
            }
            else if (p1.y < mClipRegion.y0 && p2.y < mClipRegion.y0 && p3.y < mClipRegion.y0 && p4.y < mClipRegion.y0)
            {
                // Note that Y axis is inverted by this point, have to invert it back before checking against clip
                return;
            }
            else if (p1.y > mClipRegion.y1 && p2.y > mClipRegion.y1 && p3.y > mClipRegion.y1 && p4.y > mClipRegion.y1)
            {
                return;
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            p1.z = p2.z = p3.z = p4.z = 1;

            mMeshStorage.Verticies[i + 0] = p1;
            mMeshStorage.Verticies[i + 1] = p2;
            mMeshStorage.Verticies[i + 2] = p3;
            mMeshStorage.Verticies[i + 3] = p4;

            mMeshStorage.Uvs[i + 0].x = ux0;
            mMeshStorage.Uvs[i + 0].y = uy0;
            mMeshStorage.Uvs[i + 1].x = ux1;
            mMeshStorage.Uvs[i + 1].y = uy0;
            mMeshStorage.Uvs[i + 2].x = ux1;
            mMeshStorage.Uvs[i + 2].y = uy1;
            mMeshStorage.Uvs[i + 3].x = ux0;
            mMeshStorage.Uvs[i + 3].y = uy1;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;
            mMeshStorage.Colors[i + 3] = color;

            i += 4;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;
            mMeshStorage.Indices[j++] = i - 4;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw a texture at given position, rotation
        /// </summary>
        /// <param name="srcRect">Source rectangle</param>
        /// <param name="destRect">Destination rectangle</param>
        /// <param name="pivot">Rotation pivot point</param>
        /// <param name="rotation">Rotation in degrees</param>
        /// <param name="flags">Flags</param>
        public void DrawTexture(Rect2i srcRect, Rect2i destRect, Vector2i pivot, float rotation, int flags)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            int sx0 = srcRect.x;
            int sy0 = srcRect.y;
            int sx1 = sx0 + srcRect.width;
            int sy1 = sy0 + srcRect.height;

            int dx0 = 0;
            int dy0 = 0;
            int dx1;
            int dy1;

            if ((flags & RB.ROT_90_CW) == 0)
            {
                dx1 = dx0 + destRect.width;
                dy1 = dy0 + destRect.height;
            }
            else
            {
                dx1 = dx0 + destRect.height;
                dy1 = dy0 + destRect.width;
            }

            // Wrap the angle first to values between 0 and 360
            rotation = RetroBlitUtil.WrapAngle(rotation);

            float ux0, uy0, ux1, uy1;
            float ux0raw, uy0raw, ux1raw, uy1raw;

            var spriteSheetTextureSize = SpriteSheets[CurrentSpriteSheetIndex].textureSize;

            ux0raw = sx0 / (float)spriteSheetTextureSize.x;
            uy0raw = 1.0f - (sy0 / ((float)spriteSheetTextureSize.y));
            ux1raw = sx1 / (float)spriteSheetTextureSize.x;
            uy1raw = 1.0f - (sy1 / ((float)spriteSheetTextureSize.y));

            if ((flags & RB.FLIP_H) == 0)
            {
                ux0 = ux0raw;
                ux1 = ux1raw;
            }
            else
            {
                ux0 = ux1raw;
                ux1 = ux0raw;
            }

            if ((flags & RB.FLIP_V) == 0)
            {
                uy0 = uy0raw;
                uy1 = uy1raw;
            }
            else
            {
                uy0 = uy1raw;
                uy1 = uy0raw;
            }

            Color32 color = mCurrentColor;

            Vector3 p1, p2, p3, p4;

            if ((flags & RB.ROT_90_CW) == 0)
            {
                p1 = new Vector3(dx0, dy0, 0);
                p2 = new Vector3(dx1, dy0, 0);
                p3 = new Vector3(dx1, dy1, 0);
                p4 = new Vector3(dx0, dy1, 0);
            }
            else
            {
                p1 = new Vector3(dx1, dy0, 0);
                p2 = new Vector3(dx1, dy1, 0);
                p3 = new Vector3(dx0, dy1, 0);
                p4 = new Vector3(dx0, dy0, 0);
            }

            var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotation), Vector3.one);
            matrix *= Matrix4x4.TRS(new Vector3(-pivot.x, -pivot.y, 0), Quaternion.identity, Vector3.one);

            p1 = matrix.MultiplyPoint3x4(p1);
            p2 = matrix.MultiplyPoint3x4(p2);
            p3 = matrix.MultiplyPoint3x4(p3);
            p4 = matrix.MultiplyPoint3x4(p4);

            p1.x += pivot.x;
            p1.y += pivot.y;

            p2.x += pivot.x;
            p2.y += pivot.y;

            p3.x += pivot.x;
            p3.y += pivot.y;

            p4.x += pivot.x;
            p4.y += pivot.y;

            p1.x -= mCameraPos.x - destRect.x;
            p1.y -= mCameraPos.y - destRect.y;

            p2.x -= mCameraPos.x - destRect.x;
            p2.y -= mCameraPos.y - destRect.y;

            p3.x -= mCameraPos.x - destRect.x;
            p3.y -= mCameraPos.y - destRect.y;

            p4.x -= mCameraPos.x - destRect.x;
            p4.y -= mCameraPos.y - destRect.y;

            // Early clip test
            if (p1.x < mClipRegion.x0 && p2.x < mClipRegion.x0 && p3.x < mClipRegion.x0 && p4.x < mClipRegion.x0)
            {
                return;
            }
            else if (p1.x > mClipRegion.x1 && p2.x > mClipRegion.x1 && p3.x > mClipRegion.x1 && p4.x > mClipRegion.x1)
            {
                return;
            }
            else if (p1.y < mClipRegion.y0 && p2.y < mClipRegion.y0 && p3.y < mClipRegion.y0 && p4.y < mClipRegion.y0)
            {
                // Note that Y axis is inverted by this point, have to invert it back before checking against clip
                return;
            }
            else if (p1.y > mClipRegion.y1 && p2.y > mClipRegion.y1 && p3.y > mClipRegion.y1 && p4.y > mClipRegion.y1)
            {
                return;
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            p1.z = p2.z = p3.z = p4.z = 1;

            mMeshStorage.Verticies[i + 0] = p1;
            mMeshStorage.Verticies[i + 1] = p2;
            mMeshStorage.Verticies[i + 2] = p3;
            mMeshStorage.Verticies[i + 3] = p4;

            mMeshStorage.Uvs[i + 0].x = ux0;
            mMeshStorage.Uvs[i + 0].y = uy0;
            mMeshStorage.Uvs[i + 1].x = ux1;
            mMeshStorage.Uvs[i + 1].y = uy0;
            mMeshStorage.Uvs[i + 2].x = ux1;
            mMeshStorage.Uvs[i + 2].y = uy1;
            mMeshStorage.Uvs[i + 3].x = ux0;
            mMeshStorage.Uvs[i + 3].y = uy1;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;
            mMeshStorage.Colors[i + 3] = color;

            i += 4;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;
            mMeshStorage.Indices[j++] = i - 4;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw nine-slice sprite.
        /// </summary>
        /// <param name="destRect">Destination rectangle</param>
        /// <param name="srcTopLeftCorner">Source rectangle of the top left corner</param>
        /// <param name="flagsTopLeftCorner">Render flags for top left corner</param>
        /// <param name="srcTopSide">Source rectangle of the top side</param>
        /// <param name="flagsTopSide">Render flags for top side</param>
        /// <param name="srcTopRightCorner">Source rectangle of the top right corner</param>
        /// <param name="flagsTopRightCorner">Render flaps for top right corner</param>
        /// <param name="srcLeftSide">Source rectangle of the left side</param>
        /// <param name="flagsLeftSide">Render flags for left side</param>
        /// <param name="srcMiddle">Source rectangle of the middle</param>
        /// <param name="srcRightSide">Render flags for right side</param>
        /// <param name="flagsRightSide">Source rectangle of the right side</param>
        /// <param name="srcBottomLeftCorner">Render flags for bottom left corner</param>
        /// <param name="flagsBottomLeftCorner">Source rectangle of the bottom left corner</param>
        /// <param name="srcBottomSide">Render flags for bottom side</param>
        /// <param name="flagsBottomSide">Source rectangle of the bottom side</param>
        /// <param name="srcBottomRightCorner">Render flags for bottom right corner</param>
        /// <param name="flagsBottomRightCorner">Source rectangle of the bottom right corner</param>
        public void DrawNineSlice(
            Rect2i destRect,
            Rect2i srcTopLeftCorner,
            int flagsTopLeftCorner,
            Rect2i srcTopSide,
            int flagsTopSide,
            Rect2i srcTopRightCorner,
            int flagsTopRightCorner,
            Rect2i srcLeftSide,
            int flagsLeftSide,
            Rect2i srcMiddle,
            Rect2i srcRightSide,
            int flagsRightSide,
            Rect2i srcBottomLeftCorner,
            int flagsBottomLeftCorner,
            Rect2i srcBottomSide,
            int flagsBottomSide,
            Rect2i srcBottomRightCorner,
            int flagsBottomRightCorner)
        {
            if (destRect.width < srcTopLeftCorner.width + srcBottomRightCorner.width ||
                destRect.height < srcTopLeftCorner.height + srcBottomRightCorner.height)
            {
                return;
            }

            int bottomOffset = destRect.height - srcBottomLeftCorner.height;
            int rightOffset = destRect.width - srcTopRightCorner.width;

            int xOffset = srcTopLeftCorner.width;
            while (xOffset < rightOffset && srcTopSide.width > 0)
            {
                int remainingWidth = rightOffset - xOffset;
                int width = Mathf.Min(remainingWidth, srcTopSide.width);

                // Top & Bottom horizontal
                DrawTexture(new Rect2i(srcTopSide.x, srcTopSide.y, width, srcTopSide.height), new Rect2i(destRect.x + xOffset, destRect.y, width, srcTopSide.height), flagsTopSide);
                DrawTexture(new Rect2i(srcBottomSide.x, srcBottomSide.y, width, srcBottomSide.height), new Rect2i(destRect.x + xOffset, destRect.y + bottomOffset, width, srcBottomSide.height), flagsBottomSide);

                xOffset += srcTopSide.width;
            }

            int yOffset = srcTopLeftCorner.height;
            while (yOffset < bottomOffset && srcLeftSide.height > 0)
            {
                int remainingHeight = bottomOffset - yOffset;
                int height = Mathf.Min(remainingHeight, srcLeftSide.height);

                // Left & Right verticals
                if ((flagsLeftSide & RB.ROT_90_CW) != 0)
                {
                    DrawTexture(new Rect2i(srcLeftSide.x, srcLeftSide.y, height, srcLeftSide.height), new Rect2i(destRect.x, destRect.y + yOffset, height, srcLeftSide.height), flagsLeftSide);
                    DrawTexture(new Rect2i(srcRightSide.x, srcRightSide.y, height, srcRightSide.height), new Rect2i(destRect.x + rightOffset, destRect.y + yOffset, height, srcRightSide.height), flagsRightSide);
                }
                else
                {
                    DrawTexture(new Rect2i(srcLeftSide.x, srcLeftSide.y, srcLeftSide.width, height), new Rect2i(destRect.x, destRect.y + yOffset, srcLeftSide.width, height), flagsLeftSide);
                    DrawTexture(new Rect2i(srcRightSide.x, srcRightSide.y, srcRightSide.width, height), new Rect2i(destRect.x + rightOffset, destRect.y + yOffset, srcRightSide.width, height), flagsRightSide);
                }

                yOffset += srcLeftSide.height;
            }

            yOffset = srcTopLeftCorner.height;
            while (yOffset < bottomOffset && srcMiddle.height > 0)
            {
                int remainingHeight = bottomOffset - yOffset;
                int height = Mathf.Min(remainingHeight, srcMiddle.height);

                xOffset = srcTopLeftCorner.width;
                while (xOffset < rightOffset)
                {
                    int remainingWidth = rightOffset - xOffset;
                    int width = Mathf.Min(remainingWidth, srcMiddle.width);

                    // Center
                    DrawTexture(new Rect2i(srcMiddle.x, srcMiddle.y, width, height), new Rect2i(destRect.x + xOffset, destRect.y + yOffset, width, height));

                    xOffset += srcMiddle.width;
                }

                yOffset += srcMiddle.height;
            }

            /* Top left corner */
            DrawTexture(srcTopLeftCorner, new Rect2i(destRect.x, destRect.y, srcTopLeftCorner.width, srcTopLeftCorner.height), flagsTopLeftCorner);

            /* Bottom left corner */
            DrawTexture(srcBottomLeftCorner, new Rect2i(destRect.x, destRect.y + bottomOffset, srcBottomLeftCorner.width, srcBottomLeftCorner.height), flagsBottomLeftCorner);

            /* Top right corner */
            DrawTexture(srcTopRightCorner, new Rect2i(destRect.x + rightOffset, destRect.y, srcTopRightCorner.width, srcTopRightCorner.height), flagsTopRightCorner);

            /* Bottom right corner */
            DrawTexture(srcBottomRightCorner, new Rect2i(destRect.x + rightOffset, destRect.y + bottomOffset, srcBottomRightCorner.width, srcBottomRightCorner.height), flagsBottomRightCorner);
        }

        /// <summary>
        /// Draw nine-slice sprite.
        /// </summary>
        /// <param name="destRect">Destination rectangle</param>
        /// <param name="srcTopLeftCornerID">Sprite ID of the top left corner</param>
        /// <param name="flagsTopLeftCorner">Render flags for top left corner</param>
        /// <param name="srcTopSideID">Sprite ID of the top side</param>
        /// <param name="flagsTopSide">Render flags for top side</param>
        /// <param name="srcTopRightCornerID">Sprite ID of the top right corner</param>
        /// <param name="flagsTopRightCorner">Render flaps for top right corner</param>
        /// <param name="srcLeftSideID">Sprite ID of the left side</param>
        /// <param name="flagsLeftSide">Render flags for left side</param>
        /// <param name="srcMiddleID">Sprite ID of the middle</param>
        /// <param name="srcRightSideID">Sprite ID for right side</param>
        /// <param name="flagsRightSide">Render flags for the right side</param>
        /// <param name="srcBottomLeftCornerID">Sprite ID for bottom left corner</param>
        /// <param name="flagsBottomLeftCorner">Render flags for bottom left corner</param>
        /// <param name="srcBottomSideID">Sprite ID for bottom side</param>
        /// <param name="flagsBottomSide">Render flags of bottom side</param>
        /// <param name="srcBottomRightCornerID">Sprite ID bottom right corner</param>
        /// <param name="flagsBottomRightCorner">Render flags of bottom right corner</param>
        public void DrawNineSlice(
            Rect2i destRect,
            PackedSpriteID srcTopLeftCornerID,
            int flagsTopLeftCorner,
            PackedSpriteID srcTopSideID,
            int flagsTopSide,
            PackedSpriteID srcTopRightCornerID,
            int flagsTopRightCorner,
            PackedSpriteID srcLeftSideID,
            int flagsLeftSide,
            PackedSpriteID srcMiddleID,
            PackedSpriteID srcRightSideID,
            int flagsRightSide,
            PackedSpriteID srcBottomLeftCornerID,
            int flagsBottomLeftCorner,
            PackedSpriteID srcBottomSideID,
            int flagsBottomSide,
            PackedSpriteID srcBottomRightCornerID,
            int flagsBottomRightCorner)
        {
            var srcTopLeftCorner = PackedSpriteGet(srcTopLeftCornerID.id);
            var srcTopSide = PackedSpriteGet(srcTopSideID.id);
            var srcTopRightCorner = PackedSpriteGet(srcTopRightCornerID.id);
            var srcLeftSide = PackedSpriteGet(srcLeftSideID.id);
            var srcMiddle = PackedSpriteGet(srcMiddleID.id);
            var srcRightSide = PackedSpriteGet(srcRightSideID.id);
            var srcBottomLeftCorner = PackedSpriteGet(srcBottomLeftCornerID.id);
            var srcBottomSide = PackedSpriteGet(srcBottomSideID.id);
            var srcBottomRightCorner = PackedSpriteGet(srcBottomRightCornerID.id);

            if (destRect.width < srcTopLeftCorner.Size.width + srcBottomRightCorner.Size.width ||
                destRect.height < srcTopLeftCorner.Size.height + srcBottomRightCorner.Size.height)
            {
                return;
            }

            int bottomOffset = destRect.height - srcBottomLeftCorner.Size.height;
            int rightOffset = destRect.width - srcTopRightCorner.Size.width;

            int xOffset = srcTopLeftCorner.Size.width;
            while (xOffset < rightOffset && srcTopSide.Size.width > 0)
            {
                int remainingWidth = rightOffset - xOffset;
                int width = Mathf.Min(remainingWidth, srcTopSide.Size.width);

                // Top & Bottom horizontal
                RB.DrawSprite(srcTopSide, new Rect2i(destRect.x + xOffset, destRect.y, width, srcTopSide.Size.height), flagsTopSide);
                RB.DrawSprite(srcBottomSide, new Rect2i(destRect.x + xOffset, destRect.y + bottomOffset, width, srcBottomSide.Size.height), flagsBottomSide);

                xOffset += srcTopSide.Size.width;
            }

            int yOffset = srcTopLeftCorner.Size.height;
            while (yOffset < bottomOffset && srcLeftSide.Size.height > 0)
            {
                int remainingHeight = bottomOffset - yOffset;
                int height = Mathf.Min(remainingHeight, srcLeftSide.Size.height);

                // Left & Right verticals
                if ((flagsLeftSide & RB.ROT_90_CW) != 0)
                {
                    RB.DrawSprite(srcLeftSide, new Rect2i(destRect.x, destRect.y + yOffset, height, srcLeftSide.Size.height), flagsLeftSide);
                    RB.DrawSprite(srcRightSide, new Rect2i(destRect.x + rightOffset, destRect.y + yOffset, height, srcRightSide.Size.height), flagsRightSide);
                }
                else
                {
                    RB.DrawSprite(srcLeftSide, new Rect2i(destRect.x, destRect.y + yOffset, srcLeftSide.Size.width, height), flagsLeftSide);
                    RB.DrawSprite(srcRightSide, new Rect2i(destRect.x + rightOffset, destRect.y + yOffset, srcRightSide.Size.width, height), flagsRightSide);
                }

                yOffset += srcLeftSide.Size.height;
            }

            yOffset = srcTopLeftCorner.Size.height;
            while (yOffset < bottomOffset && srcMiddle.Size.height > 0)
            {
                int remainingHeight = bottomOffset - yOffset;
                int height = Mathf.Min(remainingHeight, srcMiddle.Size.height);

                xOffset = srcTopLeftCorner.Size.width;
                while (xOffset < rightOffset)
                {
                    int remainingWidth = rightOffset - xOffset;
                    int width = Mathf.Min(remainingWidth, srcMiddle.Size.width);

                    // Center
                    RB.DrawSprite(srcMiddle, new Rect2i(destRect.x + xOffset, destRect.y + yOffset, width, height));

                    xOffset += srcMiddle.Size.width;
                }

                yOffset += srcMiddle.Size.height;
            }

            /* Top left corner */
            RB.DrawSprite(srcTopLeftCorner, new Rect2i(destRect.x, destRect.y, srcTopLeftCorner.Size.width, srcTopLeftCorner.Size.height), flagsTopLeftCorner);

            /* Bottom left corner */
            RB.DrawSprite(srcBottomLeftCorner, new Rect2i(destRect.x, destRect.y + bottomOffset, srcBottomLeftCorner.Size.width, srcBottomLeftCorner.Size.height), flagsBottomLeftCorner);

            /* Top right corner */
            RB.DrawSprite(srcTopRightCorner, new Rect2i(destRect.x + rightOffset, destRect.y, srcTopRightCorner.Size.width, srcTopRightCorner.Size.height), flagsTopRightCorner);

            /* Bottom right corner */
            RB.DrawSprite(srcBottomRightCorner, new Rect2i(destRect.x + rightOffset, destRect.y + bottomOffset, srcBottomRightCorner.Size.width, srcBottomRightCorner.Size.height), flagsBottomRightCorner);
        }

        /// <summary>
        /// Draw a single pixel
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="color">RGB color</param>
        public void DrawPixel(int x, int y, Color32 color)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 3)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            x -= mCameraPos.x;
            y -= mCameraPos.y;

            if (x < mClipRegion.x0 || x > mClipRegion.x1 || y < mClipRegion.y0 || y > mClipRegion.y1)
            {
                return;
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            // Draw pixel with just one triangle, make sure it passes through the middle of the pixel,
            // by extending its sides a bit. This should gurantee that it gets rasterized
            mMeshStorage.Verticies[i + 0].x = x;
            mMeshStorage.Verticies[i + 0].y = y;
            mMeshStorage.Verticies[i + 0].z = 0;

            mMeshStorage.Verticies[i + 1].x = x + 1.2f;
            mMeshStorage.Verticies[i + 1].y = y;
            mMeshStorage.Verticies[i + 1].z = 0;

            mMeshStorage.Verticies[i + 2].x = x;
            mMeshStorage.Verticies[i + 2].y = y + 1.2f;
            mMeshStorage.Verticies[i + 2].z = 0;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;

            i += 3;

            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw a triangle outline
        /// </summary>
        /// <param name="p0">First point of the triangle</param>
        /// <param name="p1">Second point of the triangle</param>
        /// <param name="p2">Third point of the triangle</param>
        /// <param name="color">RGB color</param>
        public void DrawTriangle(Vector2i p0, Vector2i p1, Vector2i p2, Color32 color)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            mPoints[0].x = p0.x;
            mPoints[0].y = p0.y;

            mPoints[1].x = p1.x;
            mPoints[1].y = p1.y;

            mPoints[2].x = p2.x;
            mPoints[2].y = p2.y;

            mPoints[3].x = p0.x;
            mPoints[3].y = p0.y;

            DrawLineStrip(mPoints, 4, color);
        }

        /// <summary>
        /// Draw a triangle outline
        /// </summary>
        /// <param name="p0">First point of the triangle</param>
        /// <param name="p1">Second point of the triangle</param>
        /// <param name="p2">Third point of the triangle</param>
        /// <param name="color">RGB color</param>
        /// <param name="pivot">Rotation pivot point as an offset from <paramref name="p0"/></param>
        /// <param name="rotation">Rotation in degrees</param>
        public void DrawTriangle(Vector2i p0, Vector2i p1, Vector2i p2, Color32 color, Vector2i pivot, float rotation = 0)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            rotation = RetroBlitUtil.WrapAngle(rotation);

            Vector3 fp0 = new Vector3(0, 0);
            Vector3 fp1 = new Vector3(p1.x - p0.x, p1.y - p0.y);
            Vector3 fp2 = new Vector3(p2.x - p0.x, p2.y - p0.y);

            var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotation), Vector3.one);
            matrix *= Matrix4x4.TRS(new Vector3(-pivot.x, -pivot.y, 0), Quaternion.identity, Vector3.one);

            fp0 = matrix.MultiplyPoint3x4(fp0);
            fp1 = matrix.MultiplyPoint3x4(fp1);
            fp2 = matrix.MultiplyPoint3x4(fp2);

            fp0.x += pivot.x + p0.x;
            fp0.y += pivot.y + p0.y;

            fp1.x += pivot.x + p0.x;
            fp1.y += pivot.y + p0.y;

            fp2.x += pivot.x + p0.x;
            fp2.y += pivot.y + p0.y;

            mPoints[0].x = Mathf.RoundToInt(fp0.x);
            mPoints[0].y = Mathf.RoundToInt(fp0.y);

            mPoints[1].x = Mathf.RoundToInt(fp1.x);
            mPoints[1].y = Mathf.RoundToInt(fp1.y);

            mPoints[2].x = Mathf.RoundToInt(fp2.x);
            mPoints[2].y = Mathf.RoundToInt(fp2.y);

            mPoints[3].x = Mathf.RoundToInt(fp0.x);
            mPoints[3].y = Mathf.RoundToInt(fp0.y);

            DrawLineStrip(mPoints, 4, color);
        }

        /// <summary>
        /// Draw a filled triangle
        /// </summary>
        /// <param name="p0">First point of the triangle</param>
        /// <param name="p1">Second point of the triangle</param>
        /// <param name="p2">Third point of the triangle</param>
        /// <param name="color">RGB color</param>
        public void DrawTriangleFill(Vector2i p0, Vector2i p1, Vector2i p2, Color32 color)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            p0.x -= mCameraPos.x;
            p0.y -= mCameraPos.y;

            p1.x -= mCameraPos.x;
            p1.y -= mCameraPos.y;

            p2.x -= mCameraPos.x;
            p2.y -= mCameraPos.y;

            // Early clip test
            if ((p0.x < mClipRegion.x0 && p1.x < mClipRegion.x0 && p2.x < mClipRegion.x0) ||
                (p0.y < mClipRegion.y0 && p1.y < mClipRegion.y0 && p2.y < mClipRegion.y0) ||
                (p0.x > mClipRegion.x1 && p1.x > mClipRegion.x1 && p2.x > mClipRegion.x1) ||
                (p0.y > mClipRegion.y1 && p1.y > mClipRegion.y1 && p2.y > mClipRegion.y1))
            {
                return;
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            mMeshStorage.Verticies[i + 0].x = p0.x;
            mMeshStorage.Verticies[i + 0].y = p0.y;
            mMeshStorage.Verticies[i + 0].z = 0;

            mMeshStorage.Verticies[i + 1].x = p1.x;
            mMeshStorage.Verticies[i + 1].y = p1.y;
            mMeshStorage.Verticies[i + 1].z = 0;

            mMeshStorage.Verticies[i + 2].x = p2.x;
            mMeshStorage.Verticies[i + 2].y = p2.y;
            mMeshStorage.Verticies[i + 2].z = 0;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;

            i += 4; // Skip ahead 4 even though we only used 3, this is because flush checks expects max of 6 indices per 4 verts

            // It's cheaper to draw the triangle twice in two different windings than to check triangle winding
            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 3;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw a filled triangle
        /// </summary>
        /// <param name="p0">First point of the triangle</param>
        /// <param name="p1">Second point of the triangle</param>
        /// <param name="p2">Third point of the triangle</param>
        /// <param name="color">RGB color</param>
        /// <param name="pivot">Rotation pivot point as an offset from p0</param>
        /// <param name="rotation">Rotation in degrees</param>
        public void DrawTriangleFill(Vector2i p0, Vector2i p1, Vector2i p2, Color32 color, Vector2i pivot, float rotation = 0)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            rotation = RetroBlitUtil.WrapAngle(rotation);

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            Vector3 fp0 = new Vector3(0, 0);
            Vector3 fp1 = new Vector3(p1.x - p0.x, p1.y - p0.y);
            Vector3 fp2 = new Vector3(p2.x - p0.x, p2.y - p0.y);

            if (rotation != 0)
            {
                var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotation), Vector3.one);
                matrix *= Matrix4x4.TRS(new Vector3(-pivot.x, -pivot.y, 0), Quaternion.identity, Vector3.one);

                fp0 = matrix.MultiplyPoint3x4(fp0);
                fp1 = matrix.MultiplyPoint3x4(fp1);
                fp2 = matrix.MultiplyPoint3x4(fp2);

                fp0.x += pivot.x;
                fp0.y += pivot.y;

                fp1.x += pivot.x;
                fp1.y += pivot.y;

                fp2.x += pivot.x;
                fp2.y += pivot.y;
            }

            fp0.x += -mCameraPos.x + p0.x;
            fp0.y += -mCameraPos.y + p0.y;

            fp1.x += -mCameraPos.x + p0.x;
            fp1.y += -mCameraPos.y + p0.y;

            fp2.x += -mCameraPos.x + p0.x;
            fp2.y += -mCameraPos.y + p0.y;

            // Early clip test
            if ((fp0.x < mClipRegion.x0 && fp1.x < mClipRegion.x0 && fp2.x < mClipRegion.x0) ||
                (fp0.y < mClipRegion.y0 && fp1.y < mClipRegion.y0 && fp2.y < mClipRegion.y0) ||
                (fp0.x > mClipRegion.x1 && fp1.x > mClipRegion.x1 && fp2.x > mClipRegion.x1) ||
                (fp0.y > mClipRegion.y1 && fp1.y > mClipRegion.y1 && fp2.y > mClipRegion.y1))
            {
                return;
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            fp0.z = fp1.z = fp2.z = 0;

            mMeshStorage.Verticies[i + 0] = fp0;
            mMeshStorage.Verticies[i + 1] = fp1;
            mMeshStorage.Verticies[i + 2] = fp2;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;

            i += 4; // Skip ahead 4 even though we only used 3, this is because flush checks expects max of 6 indices per 4 verts

            // It's cheaper to draw the triangle twice in two different windings than to check triangle winding
            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 3;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw a rectangle outline
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="color">RGB color</param>
        public void DrawRect(Rect2i rect, Color32 color)
        {
            int x = rect.x;
            int y = rect.y;
            int w = rect.width;
            int h = rect.height;

            if (w < 0 || h < 0)
            {
                return;
            }

            if (w <= 2 || h <= 2)
            {
                DrawRectFill(rect, color, Vector2i.zero);
            }
            else
            {
                // Check flush
                if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 3 * 4)
                {
                    Flush(FlushReason.BATCH_FULL);
                }

                int dx0 = 0;
                int dy0 = 0;
                int dx1 = dx0 + w;
                int dy1 = dy0 + h;

                // Fast color multiply
                color.r = (byte)((color.r * mCurrentColor.r) / 255);
                color.g = (byte)((color.g * mCurrentColor.g) / 255);
                color.b = (byte)((color.b * mCurrentColor.b) / 255);
                color.a = (byte)((color.a * mCurrentColor.a) / 255);

                dx0 -= mCameraPos.x - x;
                dy0 -= mCameraPos.y - y;

                dx1 -= mCameraPos.x - x;
                dy1 -= mCameraPos.y - y;

                // Early clip test
                if (dx1 < mClipRegion.x0 || dy1 < mClipRegion.y0 || dx0 > mClipRegion.x1 || dy0 > mClipRegion.y1)
                {
                    return;
                }

                int i = mMeshStorage.CurrentVertex;
                int j = mMeshStorage.CurrentIndex;

                // Top line
                mMeshStorage.Verticies[i + 0].x = dx0 + 1 - 0.1f;
                mMeshStorage.Verticies[i + 0].y = dy0 - 0.1f;
                mMeshStorage.Verticies[i + 0].z = 0;

                mMeshStorage.Verticies[i + 1].x = dx1 - 1 + 0.1f;
                mMeshStorage.Verticies[i + 1].y = dy0 + 0.5f;
                mMeshStorage.Verticies[i + 1].z = 0;

                mMeshStorage.Verticies[i + 2].x = dx0 + 1 - 0.1f;
                mMeshStorage.Verticies[i + 2].y = dy0 + 1.1f;
                mMeshStorage.Verticies[i + 2].z = 0;

                // Bottom line
                mMeshStorage.Verticies[i + 3].x = dx0 + 1 - 0.1f;
                mMeshStorage.Verticies[i + 3].y = dy1 - 1.1f;
                mMeshStorage.Verticies[i + 3].z = 0;

                mMeshStorage.Verticies[i + 4].x = dx1 - 1 + 0.1f;
                mMeshStorage.Verticies[i + 4].y = dy1 - 0.5f;
                mMeshStorage.Verticies[i + 4].z = 0;

                mMeshStorage.Verticies[i + 5].x = dx0 + 1 - 0.1f;
                mMeshStorage.Verticies[i + 5].y = dy1 + 0.1f;
                mMeshStorage.Verticies[i + 5].z = 0;

                // Left line
                mMeshStorage.Verticies[i + 6].x = dx0 - 0.1f;
                mMeshStorage.Verticies[i + 6].y = dy0 - 0.1f;
                mMeshStorage.Verticies[i + 6].z = 0;

                mMeshStorage.Verticies[i + 7].x = dx0 + 1.1f;
                mMeshStorage.Verticies[i + 7].y = dy0 - 0.1f;
                mMeshStorage.Verticies[i + 7].z = 0;

                mMeshStorage.Verticies[i + 8].x = dx0 + 0.5f;
                mMeshStorage.Verticies[i + 8].y = dy1 + 0.1f;
                mMeshStorage.Verticies[i + 8].z = 0;

                // Right line
                mMeshStorage.Verticies[i + 9].x = dx1 - 1.1f;
                mMeshStorage.Verticies[i + 9].y = dy0 - 0.1f;
                mMeshStorage.Verticies[i + 9].z = 0;

                mMeshStorage.Verticies[i + 10].x = dx1 + 0.1f;
                mMeshStorage.Verticies[i + 10].y = dy0 - 0.1f;
                mMeshStorage.Verticies[i + 10].z = 0;

                mMeshStorage.Verticies[i + 11].x = dx1 - 0.5f;
                mMeshStorage.Verticies[i + 11].y = dy1 + 0.1f;
                mMeshStorage.Verticies[i + 11].z = 0;

                mMeshStorage.Colors[i + 0] = color;
                mMeshStorage.Colors[i + 1] = color;
                mMeshStorage.Colors[i + 2] = color;

                mMeshStorage.Colors[i + 3] = color;
                mMeshStorage.Colors[i + 4] = color;
                mMeshStorage.Colors[i + 5] = color;

                mMeshStorage.Colors[i + 6] = color;
                mMeshStorage.Colors[i + 7] = color;
                mMeshStorage.Colors[i + 8] = color;

                mMeshStorage.Colors[i + 9] = color;
                mMeshStorage.Colors[i + 10] = color;
                mMeshStorage.Colors[i + 11] = color;

                mMeshStorage.Indices[j++] = i;
                mMeshStorage.Indices[j++] = i + 1;
                mMeshStorage.Indices[j++] = i + 2;

                mMeshStorage.Indices[j++] = i + 3;
                mMeshStorage.Indices[j++] = i + 1 + 3;
                mMeshStorage.Indices[j++] = i + 2 + 3;

                mMeshStorage.Indices[j++] = i + 6;
                mMeshStorage.Indices[j++] = i + 1 + 6;
                mMeshStorage.Indices[j++] = i + 2 + 6;

                mMeshStorage.Indices[j++] = i + 9;
                mMeshStorage.Indices[j++] = i + 1 + 9;
                mMeshStorage.Indices[j++] = i + 2 + 9;

                i += 12;

                mMeshStorage.CurrentVertex = i;
                mMeshStorage.CurrentIndex = j;
            }
        }

        /// <summary>
        /// Draw a rectangle outline
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="color">RGB color</param>
        /// <param name="pivot">Rotation pivot point</param>
        /// <param name="rotation">Rotation in degrees</param>
        public void DrawRect(Rect2i rect, Color32 color, Vector2i pivot, float rotation = 0)
        {
            int x = rect.x;
            int y = rect.y;
            int w = rect.width;
            int h = rect.height;

            if (w < 0 || h < 0)
            {
                return;
            }

            if (w <= 2 || h <= 2)
            {
                DrawRectFill(rect, color, Vector2i.zero);
            }
            else
            {
                rotation = RetroBlitUtil.WrapAngle(rotation);

                Vector3 p1, p2, p3, p4;

                p1 = new Vector3(0, 0, 0);
                p2 = new Vector3(w, 0, 0);
                p3 = new Vector3(w, -h, 0);
                p4 = new Vector3(0, -h, 0);

                var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotation), Vector3.one);
                matrix *= Matrix4x4.TRS(new Vector3(-pivot.x, pivot.y, 0), Quaternion.identity, Vector3.one);

                p1 = matrix.MultiplyPoint3x4(p1);
                p2 = matrix.MultiplyPoint3x4(p2);
                p3 = matrix.MultiplyPoint3x4(p3);
                p4 = matrix.MultiplyPoint3x4(p4);

                p1.x += pivot.x + x;
                p1.y -= pivot.y + y;

                p2.x += pivot.x + x;
                p2.y -= pivot.y + y;

                p3.x += pivot.x + x;
                p3.y -= pivot.y + y;

                p4.x += pivot.x + x;
                p4.y -= pivot.y + y;

                p1.y = -p1.y;
                p2.y = -p2.y;
                p3.y = -p3.y;
                p4.y = -p4.y;

                DrawLine(new Vector2i((int)p1.x, (int)p1.y), new Vector2i((int)p2.x, (int)p2.y), color, Vector2i.zero, 0);
                DrawLine(new Vector2i((int)p2.x, (int)p2.y), new Vector2i((int)p3.x, (int)p3.y), color, Vector2i.zero, 0);
                DrawLine(new Vector2i((int)p3.x, (int)p3.y), new Vector2i((int)p4.x, (int)p4.y), color, Vector2i.zero, 0);
                DrawLine(new Vector2i((int)p4.x, (int)p4.y), new Vector2i((int)p1.x, (int)p1.y), color, Vector2i.zero, 0);
            }
        }

        /// <summary>
        /// Draw a filled rectangle
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="color">RGB color</param>
        public void DrawRectFill(Rect2i rect, Color32 color)
        {
            if (rect.width <= 0 || rect.height <= 0)
            {
                return;
            }

            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            int x = rect.x;
            int y = rect.y;
            int w = rect.width;
            int h = rect.height;

            int dx0 = 0;
            int dy0 = 0;
            int dx1 = dx0 + w;
            int dy1 = dy0 + h;

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            dx0 -= mCameraPos.x - x;
            dy0 -= mCameraPos.y - y;

            dx1 -= mCameraPos.x - x;
            dy1 -= mCameraPos.y - y;

            // Early clip test
            if (dx1 < mClipRegion.x0 || dy1 < mClipRegion.y0 || dx0 > mClipRegion.x1 || dy0 > mClipRegion.y1)
            {
                return;
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            mMeshStorage.Verticies[i + 0].x = dx0;
            mMeshStorage.Verticies[i + 0].y = dy0;
            mMeshStorage.Verticies[i + 0].z = 0;

            mMeshStorage.Verticies[i + 1].x = dx1;
            mMeshStorage.Verticies[i + 1].y = dy0;
            mMeshStorage.Verticies[i + 1].z = 0;

            mMeshStorage.Verticies[i + 2].x = dx1;
            mMeshStorage.Verticies[i + 2].y = dy1;
            mMeshStorage.Verticies[i + 2].z = 0;

            mMeshStorage.Verticies[i + 3].x = dx0;
            mMeshStorage.Verticies[i + 3].y = dy1;
            mMeshStorage.Verticies[i + 3].z = 0;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;
            mMeshStorage.Colors[i + 3] = color;

            i += 4;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;
            mMeshStorage.Indices[j++] = i - 4;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw a filled rectangle
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="color">RGB color</param>
        /// <param name="pivot">Rotation pivot point</param>
        /// <param name="rotation">Rotation in degrees</param>
        public void DrawRectFill(Rect2i rect, Color32 color, Vector2i pivot, float rotation = 0)
        {
            if (rect.width <= 0 || rect.height <= 0)
            {
                return;
            }

            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            // If width or height is 1 then we're better off drawing ortho line because its made of just 1 triangle
            if ((rect.width == 1 || rect.height == 1) && rotation == 0)
            {
                DrawOrthoLine(new Vector2i(rect.x, rect.y), new Vector2i(rect.x + rect.width - 1, rect.y + rect.height - 1), color);
                return;
            }

            int x = rect.x;
            int y = rect.y;
            int w = rect.width;
            int h = rect.height;

            int dx0 = 0;
            int dy0 = 0;
            int dx1 = dx0 + w;
            int dy1 = dy0 + h;

            // Wrap the angle first to values between 0 and 360
            if (rotation != 0)
            {
                rotation = RetroBlitUtil.WrapAngle(rotation);
            }

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            Vector3 p1, p2, p3, p4;

            p1 = new Vector3(dx0, dy0, 0);
            p2 = new Vector3(dx1, dy0, 0);
            p3 = new Vector3(dx1, dy1, 0);
            p4 = new Vector3(dx0, dy1, 0);

            if (rotation != 0)
            {
                var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotation), Vector3.one);
                matrix *= Matrix4x4.TRS(new Vector3(-pivot.x, -pivot.y, 0), Quaternion.identity, Vector3.one);

                p1 = matrix.MultiplyPoint3x4(p1);
                p2 = matrix.MultiplyPoint3x4(p2);
                p3 = matrix.MultiplyPoint3x4(p3);
                p4 = matrix.MultiplyPoint3x4(p4);

                p1.x += pivot.x;
                p1.y += pivot.y;

                p2.x += pivot.x;
                p2.y += pivot.y;

                p3.x += pivot.x;
                p3.y += pivot.y;

                p4.x += pivot.x;
                p4.y += pivot.y;
            }

            p1.x -= mCameraPos.x - x;
            p1.y -= mCameraPos.y - y;

            p2.x -= mCameraPos.x - x;
            p2.y -= mCameraPos.y - y;

            p3.x -= mCameraPos.x - x;
            p3.y -= mCameraPos.y - y;

            p4.x -= mCameraPos.x - x;
            p4.y -= mCameraPos.y - y;

            // Early clip test
            if (p1.x < mClipRegion.x0 && p2.x < mClipRegion.x0 && p3.x < mClipRegion.x0 && p4.x < mClipRegion.x0)
            {
                return;
            }
            else if (p1.x > mClipRegion.x1 && p2.x > mClipRegion.x1 && p3.x > mClipRegion.x1 && p4.x > mClipRegion.x1)
            {
                return;
            }
            else if (p1.y < mClipRegion.y0 && p2.y < mClipRegion.y0 && p3.y < mClipRegion.y0 && p4.y < mClipRegion.y0)
            {
                // Note that Y axis is inverted by this point, have to invert it back before checking against clip
                return;
            }
            else if (p1.y > mClipRegion.y1 && p2.y > mClipRegion.y1 && p3.y > mClipRegion.y1 && p4.y > mClipRegion.y1)
            {
                return;
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            p1.z = p2.z = p3.z = p4.z = 0;

            mMeshStorage.Verticies[i + 0] = p1;
            mMeshStorage.Verticies[i + 1] = p2;
            mMeshStorage.Verticies[i + 2] = p3;
            mMeshStorage.Verticies[i + 3] = p4;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;
            mMeshStorage.Colors[i + 3] = color;

            i += 4;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;
            mMeshStorage.Indices[j++] = i - 4;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw a vertical line with no checks
        /// </summary>
        /// <param name="x">Start x</param>
        /// <param name="y1">Start y</param>
        /// <param name="y2">End y</param>
        public void DrawVerticalLineNoChecks(int x, int y1, int y2)
        {
            x -= mCameraPos.x;
            y1 -= mCameraPos.y;
            y2 -= mCameraPos.y;

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            mMeshStorage.Verticies[i + 0].x = x - 0.1f;
            mMeshStorage.Verticies[i + 0].y = y1 - 0.1f;
            mMeshStorage.Verticies[i + 0].z = 0;

            mMeshStorage.Verticies[i + 1].x = x + 1.1f;
            mMeshStorage.Verticies[i + 1].y = y1 - 0.1f;
            mMeshStorage.Verticies[i + 1].z = 0;

            mMeshStorage.Verticies[i + 2].x = x + 0.5f;
            mMeshStorage.Verticies[i + 2].y = y2 + 1.1f;
            mMeshStorage.Verticies[i + 2].z = 0;

            mMeshStorage.Colors[i + 0] = mCurrentColor;
            mMeshStorage.Colors[i + 1] = mCurrentColor;
            mMeshStorage.Colors[i + 2] = mCurrentColor;

            i += 3;

            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw a vertical line with no checks
        /// </summary>
        /// <param name="x1">Start x</param>
        /// <param name="x2">End x</param>
        /// <param name="y">Start y</param>
        public void DrawHorizontalLineNoChecks(int x1, int x2, int y)
        {
            x1 -= mCameraPos.x;
            x2 -= mCameraPos.x;
            y -= mCameraPos.y;

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            mMeshStorage.Verticies[i + 0].x = x1 - 0.1f;
            mMeshStorage.Verticies[i + 0].y = y - 0.1f;
            mMeshStorage.Verticies[i + 0].z = 0;

            mMeshStorage.Verticies[i + 1].x = x2 + 1.1f;
            mMeshStorage.Verticies[i + 1].y = y + 0.5f;
            mMeshStorage.Verticies[i + 1].z = 0;

            mMeshStorage.Verticies[i + 2].x = x1 - 0.1f;
            mMeshStorage.Verticies[i + 2].y = y + 1.1f;
            mMeshStorage.Verticies[i + 2].z = 0;

            mMeshStorage.Colors[i + 0] = mCurrentColor;
            mMeshStorage.Colors[i + 1] = mCurrentColor;
            mMeshStorage.Colors[i + 2] = mCurrentColor;

            i += 3;

            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw a rect fill with no checks
        /// </summary>
        /// <param name="x1">Start x</param>
        /// <param name="y1">Start y</param>
        /// <param name="x2">End x</param>
        /// <param name="y2">End y</param>
        public void DrawRectFillNoChecks(int x1, int y1, int x2, int y2)
        {
            x1 -= mCameraPos.x;
            x2 -= mCameraPos.x;
            y1 -= mCameraPos.y;
            y2 -= mCameraPos.y;

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            mMeshStorage.Verticies[i + 0].x = x1;
            mMeshStorage.Verticies[i + 0].y = y1;
            mMeshStorage.Verticies[i + 0].z = 0;

            mMeshStorage.Verticies[i + 1].x = x2;
            mMeshStorage.Verticies[i + 1].y = y1;
            mMeshStorage.Verticies[i + 1].z = 0;

            mMeshStorage.Verticies[i + 2].x = x2;
            mMeshStorage.Verticies[i + 2].y = y2;
            mMeshStorage.Verticies[i + 2].z = 0;

            mMeshStorage.Verticies[i + 3].x = x1;
            mMeshStorage.Verticies[i + 3].y = y2;
            mMeshStorage.Verticies[i + 3].z = 0;

            mMeshStorage.Colors[i + 0] = mCurrentColor;
            mMeshStorage.Colors[i + 1] = mCurrentColor;
            mMeshStorage.Colors[i + 2] = mCurrentColor;
            mMeshStorage.Colors[i + 3] = mCurrentColor;

            i += 4;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;
            mMeshStorage.Indices[j++] = i - 4;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw line from lp0 to lp1
        /// </summary>
        /// <param name="lp0">Start point</param>
        /// <param name="lp1">End point</param>
        /// <param name="color">RGB color</param>
        public void DrawLine(Vector2i lp0, Vector2i lp1, Color32 color)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            // Trivial straight lines, use ortho, faster
            if (lp0.x == lp1.x || lp0.y == lp1.y)
            {
                DrawOrthoLine(lp0, lp1, color);
                return;
            }

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            Vector3 lp0f = new Vector3(lp0.x, lp0.y, 0);
            Vector3 lp1f = new Vector3(lp1.x, lp1.y, 0);

            lp0f.x -= mCameraPos.x;
            lp0f.y -= mCameraPos.y;

            lp1f.x -= mCameraPos.x;
            lp1f.y -= mCameraPos.y;

            // Early clip test
            if (lp0f.x < mClipRegion.x0 && lp1f.x < mClipRegion.x0)
            {
                return;
            }
            else if (lp0f.x > mClipRegion.x1 && lp1f.x > mClipRegion.x1)
            {
                return;
            }
            else if (lp0f.y < mClipRegion.y0 && lp1f.y < mClipRegion.y0)
            {
                return;
            }
            else if (lp0f.y > mClipRegion.y1 && lp1f.y > mClipRegion.y1)
            {
                return;
            }

            Vector2 dir = lp1f - lp0f;
            Vector3 p0, p1, p2, p3;

            /* Figure out which quadrant the angle is in
             *
             * \      0     /
             *   \        /
             *     \    /
             *       \/
             * 3     /\     1
             *     /    \
             *   /        \
             * /      2     \
             */
            if (System.Math.Abs(dir.x) > System.Math.Abs(dir.y))
            {
                if (dir.x > 0)
                {
                    // quadrant 1
                    p0 = lp0f;
                    p1 = lp1f;

                    p0.x += 0.5f;
                    p1.x += 0.5f;
                    p2 = new Vector2(p1.x, p1.y + 1.0f);
                    p3 = new Vector2(p0.x, p0.y + 1.0f);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    p0 += sideDir * -0.5f;
                    p3 += sideDir * -0.5f;

                    p1 += sideDir * 0.5f;
                    p2 += sideDir * 0.5f;
                }
                else
                {
                    // quadrant 3
                    p1 = lp0f;
                    p0 = lp1f;

                    p0.x += 0.5f;
                    p1.x += 0.5f;
                    p2 = new Vector2(p1.x, p1.y + 1.0f);
                    p3 = new Vector2(p0.x, p0.y + 1.0f);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    p1 += sideDir * 0.5f;
                    p2 += sideDir * 0.5f;

                    p0 += sideDir * -0.5f;
                    p3 += sideDir * -0.5f;
                }
            }
            else
            {
                if (dir.y < 0)
                {
                    // quadrant 0
                    p0 = lp0f;
                    p1 = lp1f;

                    p0.y += 0.5f;
                    p1.y += 0.5f;
                    p2 = new Vector2(p1.x + 1.0f, p1.y);
                    p3 = new Vector2(p0.x + 1.0f, p0.y);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    p0 += sideDir * -0.5f;
                    p3 += sideDir * -0.5f;

                    p1 += sideDir * 0.5f;
                    p2 += sideDir * 0.5f;
                }
                else
                {
                    // quadrant 2
                    p1 = lp0f;
                    p0 = lp1f;

                    p0.y += 0.5f;
                    p1.y += 0.5f;
                    p2 = new Vector2(p1.x + 1.0f, p1.y);
                    p3 = new Vector2(p0.x + 1.0f, p0.y);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    p1 += sideDir * 0.5f;
                    p2 += sideDir * 0.5f;

                    p0 += sideDir * -0.5f;
                    p3 += sideDir * -0.5f;
                }
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            p0.z = p1.z = p2.z = p3.z = 0;

            mMeshStorage.Verticies[i + 0] = p0;
            mMeshStorage.Verticies[i + 1] = p1;
            mMeshStorage.Verticies[i + 2] = p2;
            mMeshStorage.Verticies[i + 3] = p3;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;
            mMeshStorage.Colors[i + 3] = color;

            i += 4;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;
            mMeshStorage.Indices[j++] = i - 4;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw line from lp0 to lp1
        /// </summary>
        /// <param name="lp0">Start point</param>
        /// <param name="lp1">End point</param>
        /// <param name="color">RGB color</param>
        /// <param name="pivot">Rotation pivot point</param>
        /// <param name="rotation">Rotation in degrees</param>
        public void DrawLine(Vector2i lp0, Vector2i lp1, Color32 color, Vector2i pivot, float rotation = 0)
        {
            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            rotation = RetroBlitUtil.WrapAngle(rotation);

            if (lp0.x == lp1.x || lp0.y == lp1.y)
            {
                DrawOrthoLine(lp0, lp1, color);
                return;
            }

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            Vector3 lp0f = new Vector3(lp0.x, lp0.y, 0);
            Vector3 lp1f = new Vector3(lp1.x, lp1.y, 0);

            var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotation), Vector3.one);
            matrix *= Matrix4x4.TRS(new Vector3(-pivot.x, -pivot.y, 0), Quaternion.identity, Vector3.one);

            lp0f = matrix.MultiplyPoint3x4(lp0f);
            lp1f = matrix.MultiplyPoint3x4(lp1f);

            lp0f.x += pivot.x;
            lp0f.y += pivot.y;

            lp1f.x += pivot.x;
            lp1f.y += pivot.y;

            lp0f.x -= mCameraPos.x;
            lp0f.y -= mCameraPos.y;

            lp1f.x -= mCameraPos.x;
            lp1f.y -= mCameraPos.y;

            // Early clip test
            if (lp0f.x < mClipRegion.x0 && lp1f.x < mClipRegion.x0)
            {
                return;
            }
            else if (lp0f.x > mClipRegion.x1 && lp1f.x > mClipRegion.x1)
            {
                return;
            }
            else if (lp0f.y < mClipRegion.y0 && lp1f.y < mClipRegion.y0)
            {
                return;
            }
            else if (lp0f.y > mClipRegion.y1 && lp1f.y > mClipRegion.y1)
            {
                return;
            }

            Vector2 dir = lp1f - lp0f;
            Vector3 p0, p1, p2, p3;

            /* Figure out which quadrant the angle is in
             *
             * \      0     /
             *   \        /
             *     \    /
             *       \/
             * 3     /\     1
             *     /    \
             *   /        \
             * /      2     \
             */
            if (System.Math.Abs(dir.x) > System.Math.Abs(dir.y))
            {
                    if (dir.x > 0)
                {
                    // quadrant 1
                    p0 = lp0f;
                    p1 = lp1f;

                    p0.x += 0.5f;
                    p1.x += 0.5f;
                    p2 = new Vector2(p1.x, p1.y + 1.0f);
                    p3 = new Vector2(p0.x, p0.y + 1.0f);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    p0 += sideDir * -0.5f;
                    p3 += sideDir * -0.5f;

                    p1 += sideDir * 0.5f;
                    p2 += sideDir * 0.5f;
                }
                else
                {
                    // quadrant 3
                    p1 = lp0f;
                    p0 = lp1f;

                    p0.x += 0.5f;
                    p1.x += 0.5f;
                    p2 = new Vector2(p1.x, p1.y + 1.0f);
                    p3 = new Vector2(p0.x, p0.y + 1.0f);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    p1 += sideDir * 0.5f;
                    p2 += sideDir * 0.5f;

                    p0 += sideDir * -0.5f;
                    p3 += sideDir * -0.5f;
                }
            }
            else
            {
                if (dir.y < 0)
                {
                    // quadrant 0
                    p0 = lp0f;
                    p1 = lp1f;

                    p0.y += 0.5f;
                    p1.y += 0.5f;
                    p2 = new Vector2(p1.x + 1.0f, p1.y);
                    p3 = new Vector2(p0.x + 1.0f, p0.y);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    p0 += sideDir * -0.5f;
                    p3 += sideDir * -0.5f;

                    p1 += sideDir * 0.5f;
                    p2 += sideDir * 0.5f;
                }
                else
                {
                    // quadrant 2
                    p1 = lp0f;
                    p0 = lp1f;

                    p0.y += 0.5f;
                    p1.y += 0.5f;
                    p2 = new Vector2(p1.x + 1.0f, p1.y);
                    p3 = new Vector2(p0.x + 1.0f, p0.y);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    p1 += sideDir * 0.5f;
                    p2 += sideDir * 0.5f;

                    p0 += sideDir * -0.5f;
                    p3 += sideDir * -0.5f;
                }
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            p0.z = p1.z = p2.z = p3.z = 0;

            mMeshStorage.Verticies[i + 0] = p0;
            mMeshStorage.Verticies[i + 1] = p1;
            mMeshStorage.Verticies[i + 2] = p2;
            mMeshStorage.Verticies[i + 3] = p3;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;
            mMeshStorage.Colors[i + 3] = color;

            i += 4;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;
            mMeshStorage.Indices[j++] = i - 4;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw line from lp0 to lp1
        /// </summary>
        /// <param name="lp0">Start point</param>
        /// <param name="lp1">End point</param>
        /// <param name="color">RGB color</param>
        /// <param name="startPixel">Render the first pixel in the line</param>
        /// <param name="endPixel">Render the last pixel in the line</param>
        public void DrawLineWithCaps(Vector2i lp0, Vector2i lp1, Color32 color, bool startPixel = true, bool endPixel = true)
        {
            //// CheckFlush should already be called by caller

            if (lp0.x == lp1.x && lp0.y == lp1.y)
            {
                if (!startPixel && !endPixel)
                {
                    // Do nothing
                    return;
                }

                DrawPixel(lp0.x, lp0.y, color);
                return;
            }

            // Trivial straight lines, use rect, faster
            if (lp0.x == lp1.x)
            {
                if (!startPixel)
                {
                    if (lp0.y < lp1.y)
                    {
                        lp0.y++;
                    }
                    else
                    {
                        lp0.y--;
                    }
                }

                if (!endPixel)
                {
                    if (lp0.y < lp1.y)
                    {
                        lp1.y--;
                    }
                    else
                    {
                        lp1.y++;
                    }
                }

                DrawOrthoLine(lp0, lp1, color);
                return;
            }

            if (lp0.y == lp1.y)
            {
                if (!startPixel)
                {
                    if (lp0.x < lp1.x)
                    {
                        lp0.x++;
                    }
                    else
                    {
                        lp0.x--;
                    }
                }

                if (!endPixel)
                {
                    if (lp0.x < lp1.x)
                    {
                        lp1.x--;
                    }
                    else
                    {
                        lp1.x++;
                    }
                }

                DrawOrthoLine(lp0, lp1, color);
                return;
            }

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            lp0.x -= mCameraPos.x;
            lp0.y -= mCameraPos.y;

            lp1.x -= mCameraPos.x;
            lp1.y -= mCameraPos.y;

            // Early clip test
            if (lp0.x < mClipRegion.x0 && lp1.x < mClipRegion.x0)
            {
                return;
            }
            else if (lp0.x > mClipRegion.x1 && lp1.x > mClipRegion.x1)
            {
                return;
            }
            else if (lp0.y < mClipRegion.y0 && lp1.y < mClipRegion.y0)
            {
                return;
            }
            else if (lp0.y > mClipRegion.y1 && lp1.y > mClipRegion.y1)
            {
                return;
            }

            Vector2i dir = lp1 - lp0;
            Vector3 p0, p1, p2, p3;

            /* Figure out which quadrant the angle is in
             *
             * \      0     /
             *   \        /
             *     \    /
             *       \/
             * 3     /\     1
             *     /    \
             *   /        \
             * /      2     \
             */
            if (RetroBlitUtil.FastIntAbs(dir.x) > RetroBlitUtil.FastIntAbs(dir.y))
            {
                if (dir.x > 0)
                {
                    // quadrant 1
                    p0 = new Vector2(lp0.x + 0.5f, lp0.y);
                    p1 = new Vector2(lp1.x + 0.5f, lp1.y);
                    p2 = new Vector2(p1.x, p1.y + 1.0f);
                    p3 = new Vector2(p0.x, p0.y + 1.0f);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    if (startPixel)
                    {
                        p0 += sideDir * -0.5f;
                        p3 += sideDir * -0.5f;
                    }
                    else
                    {
                        p0 += sideDir * 0.5f;
                        p3 += sideDir * 0.5f;
                    }

                    if (endPixel)
                    {
                        p1 += sideDir * 0.5f;
                        p2 += sideDir * 0.5f;
                    }
                    else
                    {
                        p1 += sideDir * -0.5f;
                        p2 += sideDir * -0.5f;
                    }
                }
                else
                {
                    // quadrant 3
                    p0 = new Vector2(lp1.x + 0.5f, lp1.y);
                    p1 = new Vector2(lp0.x + 0.5f, lp0.y);
                    p2 = new Vector2(p1.x, p1.y + 1.0f);
                    p3 = new Vector2(p0.x, p0.y + 1.0f);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    if (startPixel)
                    {
                        p1 += sideDir * 0.5f;
                        p2 += sideDir * 0.5f;
                    }
                    else
                    {
                        p1 += sideDir * -0.5f;
                        p2 += sideDir * -0.5f;
                    }

                    if (endPixel)
                    {
                        p0 += sideDir * -0.5f;
                        p3 += sideDir * -0.5f;
                    }
                    else
                    {
                        p0 += sideDir * 0.5f;
                        p3 += sideDir * 0.5f;
                    }
                }
            }
            else
            {
                if (dir.y < 0)
                {
                    // quadrant 0
                    p0 = new Vector2(lp0.x, lp0.y + 0.5f);
                    p1 = new Vector2(lp1.x, lp1.y + 0.5f);
                    p2 = new Vector2(p1.x + 1.0f, p1.y);
                    p3 = new Vector2(p0.x + 1.0f, p0.y);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    if (startPixel)
                    {
                        p0 += sideDir * -0.5f;
                        p3 += sideDir * -0.5f;
                    }
                    else
                    {
                        p0 += sideDir * 0.5f;
                        p3 += sideDir * 0.5f;
                    }

                    if (endPixel)
                    {
                        p1 += sideDir * 0.5f;
                        p2 += sideDir * 0.5f;
                    }
                    else
                    {
                        p1 += sideDir * -0.5f;
                        p2 += sideDir * -0.5f;
                    }
                }
                else
                {
                    // quadrant 2
                    p0 = new Vector2(lp1.x, lp1.y + 0.5f);
                    p1 = new Vector2(lp0.x, lp0.y + 0.5f);
                    p2 = new Vector2(p1.x + 1.0f, p1.y);
                    p3 = new Vector2(p0.x + 1.0f, p0.y);

                    var sideDir = p1 - p0;
                    sideDir.Normalize();

                    if (startPixel)
                    {
                        p1 += sideDir * 0.5f;
                        p2 += sideDir * 0.5f;
                    }
                    else
                    {
                        p1 += sideDir * -0.5f;
                        p2 += sideDir * -0.5f;
                    }

                    if (endPixel)
                    {
                        p0 += sideDir * -0.5f;
                        p3 += sideDir * -0.5f;
                    }
                    else
                    {
                        p0 += sideDir * 0.5f;
                        p3 += sideDir * 0.5f;
                    }
                }
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            p0.z = p1.z = p2.z = p3.z = 0;

            mMeshStorage.Verticies[i + 0] = p0;
            mMeshStorage.Verticies[i + 1] = p1;
            mMeshStorage.Verticies[i + 2] = p2;
            mMeshStorage.Verticies[i + 3] = p3;

            mMeshStorage.Colors[i + 0] = color;
            mMeshStorage.Colors[i + 1] = color;
            mMeshStorage.Colors[i + 2] = color;
            mMeshStorage.Colors[i + 3] = color;

            i += 4;

            mMeshStorage.Indices[j++] = i - 4;
            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;

            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;
            mMeshStorage.Indices[j++] = i - 4;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        /// <summary>
        /// Draw ellipse
        /// </summary>
        /// <param name="center">Center of ellipse</param>
        /// <param name="radius">Radius</param>
        /// <param name="color">RGB color</param>
        public void DrawEllipse(Vector2i center, Vector2i radius, Color32 color)
        {
            Rect2i userClipRegion = RB.ClipGet();
            Rect2i bounds = new Rect2i(center.x - radius.x - 1, center.y - radius.y - 1, (radius.x * 2) + 2, (radius.y * 2) + 2);
            var cameraPos = RB.CameraGet();
            bounds.x -= cameraPos.x;
            bounds.y -= cameraPos.y;
            if (!userClipRegion.Intersects(bounds))
            {
                return;
            }

            if (radius.x <= 0 || radius.y <= 0)
            {
                return;
            }

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            DrawEllipseInternal(center, radius, color);
        }

        /// <summary>
        /// Draw filled ellipse
        /// </summary>
        /// <param name="center">Center of ellipse</param>
        /// <param name="radius">Radius</param>
        /// <param name="color">RGB color</param>
        /// <param name="inverse">Do an inverted fill?</param>
        public void DrawEllipseFill(Vector2i center, Vector2i radius, Color32 color, bool inverse)
        {
            Rect2i userClipRegion = RB.ClipGet();
            Rect2i bounds = new Rect2i(center.x - radius.x - 1, center.y - radius.y - 1, (radius.x * 2) + 2, (radius.y * 2) + 2);
            var cameraPos = RB.CameraGet();
            bounds.x -= cameraPos.x;
            bounds.y -= cameraPos.y;
            if (!userClipRegion.Intersects(bounds))
            {
                return;
            }

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            if (!inverse)
            {
                DrawEllipseFillInternal(center, radius, color);
            }
            else
            {
                DrawEllipseFillInverseInternal(center, radius, color);
            }
        }

        /// <summary>
        /// Draw a prepared mesh to screen
        /// </summary>
        /// <param name="mesh">Mesh to draw</param>
        /// <param name="drawPos">Draw position</param>
        /// <param name="rect">Rect to check against clip region</param>
        /// <param name="translateToCamera">Apply camera offset</param>
        /// <param name="texture">Texture to render the mesh with</param>
        public void DrawPreparedMesh(Mesh mesh, Vector2i drawPos, Rect2i rect, bool translateToCamera, Texture texture)
        {
            if (!RenderEnabled)
            {
                return;
            }

            if (mesh == null)
            {
                return;
            }

            // Early clip test
            var clipRect = new Rect2i(mClipRegion.x0, mClipRegion.y0, mClipRegion.x1 - mClipRegion.x0, mClipRegion.y1 - mClipRegion.y0);

            if (!rect.Intersects(clipRect))
            {
                return;
            }

            Flush(FlushReason.TILEMAP_CHUNK);

            SetShaderValues();
            SetShaderGlobalTint(mCurrentColor);

            Graphics.SetRenderTarget(mCurrentRenderTexture);
            SetCurrentTexture(texture, mEmptySpriteSheet, false);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, mCurrentRenderTexture.width, mCurrentRenderTexture.height, 0);

            Vector3 posFinal;
            if (translateToCamera)
            {
                posFinal = new Vector3(-mCameraPos.x, -mCameraPos.y, 0);
            }
            else
            {
                posFinal = Vector3.zero;
            }

            posFinal.x += drawPos.x;
            posFinal.y += drawPos.y;

            for (int pass = 0; pass < mCurrentDrawMaterial.passCount; pass++)
            {
                mFlushInfo[(int)FlushReason.TILEMAP_CHUNK].Count++;
                mCurrentDrawMaterial.SetPass(pass);

                Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(posFinal, Quaternion.identity, Vector3.one));
            }

            if (CurrentSpriteSheetIndex != DUMMY_SPRITESHEET_INDEX)
            {
                SetCurrentTexture(SpriteSheets[CurrentSpriteSheetIndex].texture, SpriteSheets[CurrentSpriteSheetIndex], false);
            }
            else
            {
                SetCurrentTexture(null, mEmptySpriteSheet, false);
            }

            SetShaderGlobalTint(Color.white);

            GL.PopMatrix();
        }

        /// <summary>
        /// Set camera position
        /// </summary>
        /// <param name="pos">Camera position</param>
        public void CameraSet(Vector2i pos)
        {
            mCameraPos = pos;
        }

        /// <summary>
        /// Get camera position
        /// </summary>
        /// <returns>Camera position</returns>
        public Vector2i CameraGet()
        {
            return mCameraPos;
        }

        /// <summary>
        /// Start renderer for the frame
        /// </summary>
        public void StartRender()
        {
            if (!RenderEnabled)
            {
                return;
            }

            ResetMesh();

            mFrontBuffer.Reset();
            ShaderReset();

            mPreviousTexture = null;

            mDebugClipRegions.Clear();

            Onscreen();
            CameraSet(Vector2i.zero);
            AlphaSet(255);
            TintColorSet(new Color32(255, 255, 255, 255));
            RB.ClipReset();

            mCurrentBatchSpriteIndex = CurrentSpriteSheetIndex;

            if (CurrentSpriteSheetIndex != DUMMY_SPRITESHEET_INDEX)
            {
                SetCurrentTexture(SpriteSheets[CurrentSpriteSheetIndex].texture, SpriteSheets[CurrentSpriteSheetIndex], true);
            }
            else
            {
                SetCurrentTexture(null, mEmptySpriteSheet, true);
            }
        }

        /// <summary>
        /// End renderer for the frame. This also applies some renderer based post-processing effects by drawing
        /// on top of anything else the user may have drawn
        /// </summary>
        public void FrameEnd()
        {
            if (!RenderEnabled)
            {
                return;
            }

            Flush(FlushReason.FRAME_END);

            var drawState = StoreState();

            Onscreen();
            CameraSet(Vector2i.zero);
            AlphaSet(255);
            TintColorSet(new Color32(255, 255, 255, 255));
            RB.ClipReset();
            ShaderReset();

            mRetroBlitAPI.Effects.ApplyRenderTimeEffects();

            mFrontBuffer.FrameEnd(mRetroBlitAPI);

            if (mShowFlushDebug)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                int lineCount = 0;
                for (int i = 0; i < mFlushInfo.Length; i++)
                {
                    if (mFlushInfo[i].Count > 0)
                    {
                        if (lineCount > 0)
                        {
                            sb.Append("\n");
                        }

                        sb.Append(mFlushInfo[i].Reason);
                        sb.Append(": ");
                        sb.Append(mFlushInfo[i].Count);

                        mFlushInfo[i].Count = 0;

                        lineCount++;
                    }
                }

                string flushString = sb.ToString();
                var flushStringSize = RB.PrintMeasure(flushString);
                RB.DrawRectFill(new Rect2i(0, 0, flushStringSize.x + 8, flushStringSize.y + 8), mFlushDebugBackgroundColor);
                RB.Print(new Vector2i(4, 4), mFlushDebugFontColor, sb.ToString());
            }

#if EVAL_ONLY
            var evalSize = RB.PrintMeasure("Evaluation Only!");
            var evalRect = new Rect2i(
                RB.DisplaySize.width - evalSize.width - 2,
                RB.DisplaySize.height - evalSize.height - 2,
                evalSize.width + 4,
                evalSize.height + 4);

            RB.DrawRectFill(evalRect, Color.black);
            RB.Print(new Vector2i(evalRect.x + 1, evalRect.y + 1), Color.white, "@w144Evaluation Only!");
#endif

            Flush(FlushReason.FRAME_END);

            RestoreState(drawState);

            mFlushInfo[(int)FlushReason.EFFECT_APPLY].Count++;
        }

        /// <summary>
        /// Draw all clip regions
        /// </summary>
        public void DrawClipRegions()
        {
            for (int i = 0; i < mDebugClipRegions.Count; i++)
            {
                DrawRect(mDebugClipRegions[i].region, mDebugClipRegions[i].color, Vector2i.zero);
            }
        }

        /// <summary>
        /// Set clip region
        /// </summary>
        /// <param name="rect">Region</param>
        public void ClipSet(Rect2i rect)
        {
            Rect2i origRect = rect;

            if (rect.width < 0 || rect.height < 0)
            {
                return;
            }

            int x0 = rect.x;
            int y0 = rect.y;
            int x1 = x0 + rect.width - 1;
            int y1 = y0 + rect.height - 1;

            if (x0 != mClipRegion.x0 || x1 != mClipRegion.x1 || y0 != mClipRegion.y0 || y1 != mClipRegion.y1)
            {
                Flush(FlushReason.CLIP_CHANGE);

                mClipRegion.x0 = x0;
                mClipRegion.y0 = y0;
                mClipRegion.x1 = x1;
                mClipRegion.y1 = y1;
            }

            mClip = rect;

            if (mClipDebug)
            {
                DebugClipRegion region;
                region.region = origRect;
                region.color = mClipDebugColor;

                mDebugClipRegions.Add(region);
            }
        }

        /// <summary>
        /// Get clip region
        /// </summary>
        /// <returns>Clip region</returns>
        public Rect2i ClipGet()
        {
            return mClip;
        }

        /// <summary>
        /// Set clip debug state
        /// </summary>
        /// <param name="enabled">Enable/Disabled flag</param>
        /// <param name="color">RGBA color</param>
        public void ClipDebugSet(bool enabled, Color32 color)
        {
            mClipDebug = enabled;
            mClipDebugColor = color;
        }

        /// <summary>
        /// Set flush debug state
        /// </summary>
        /// <param name="enabled">Enabled/Disabled flag</param>
        /// <param name="fontColor">Font RGBA color</param>
        /// <param name="backgroundColor">Background RGBA color</param>
        public void FlashDebugSet(bool enabled, Color32 fontColor, Color32 backgroundColor)
        {
            mFlushDebugBackgroundColor = backgroundColor;
            mFlushDebugFontColor = fontColor;
            mShowFlushDebug = enabled;
        }

        /// <summary>
        /// Set alpha transparency
        /// </summary>
        /// <param name="a">Alpha value</param>
        public void AlphaSet(byte a)
        {
            mCurrentColor.a = a;
        }

        /// <summary>
        /// Get alpha transparency value
        /// </summary>
        /// <returns>Alpha value</returns>
        public byte AlphaGet()
        {
            return mCurrentColor.a;
        }

        /// <summary>
        /// Set Tint color to apply to drawing. Alpha ignored, use AlphaSet
        /// </summary>
        /// <param name="tintColor">Tint color</param>
        public void TintColorSet(Color32 tintColor)
        {
            mCurrentColor.r = tintColor.r;
            mCurrentColor.g = tintColor.g;
            mCurrentColor.b = tintColor.b;
        }

        /// <summary>
        /// Get current tint color
        /// </summary>
        /// <returns>Tint color</returns>
        public Color32 TintColorGet()
        {
            return new Color32(mCurrentColor.r, mCurrentColor.g, mCurrentColor.b, 255);
        }

        /// <summary>
        /// Create render texture
        /// </summary>
        /// <param name="size">Dimensions</param>
        /// <returns>Render texture</returns>
        public RenderTexture RenderTextureCreate(Vector2i size)
        {
            RenderTexture tex = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            if (tex == null)
            {
                return null;
            }

            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.anisoLevel = 0;
            tex.antiAliasing = 1;

            tex.autoGenerateMips = false;
            tex.depth = 0;
            tex.useMipMap = false;

            tex.Create();

            return tex;
        }

        /// <summary>
        /// Set the offscreen target by spritesheet target index, also resets the clipping region to cover the new render target
        /// </summary>
        /// <param name="spriteSheetIndex">Spritesheet index</param>
        public void OffscreenTarget(int spriteSheetIndex)
        {
            if (mCurrentRenderTexture != SpriteSheets[spriteSheetIndex].texture)
            {
                Flush(FlushReason.OFFSCREEN_CHANGE);
            }

            mCurrentRenderTexture = SpriteSheets[spriteSheetIndex].texture;
            RB.ClipReset();

            mRetroBlitAPI.PixelCamera.SetRenderTarget(mCurrentRenderTexture);

            if (SpriteSheets[spriteSheetIndex].needsClear)
            {
                Clear(new Color32(0, 0, 0, 0));
                SpriteSheets[spriteSheetIndex].needsClear = false;
            }
        }

        /// <summary>
        /// Get current render texture
        /// </summary>
        /// <returns>Render texture</returns>
        public Texture CurrentRenderTexture()
        {
            return mCurrentRenderTexture;
        }

        /// <summary>
        /// Check if given sprite sheet is valid
        /// </summary>
        /// <param name="spritesheetIndex">Sprite sheet index</param>
        /// <returns>True if valid</returns>
        public bool SpriteSheetValid(int spritesheetIndex)
        {
            if (spritesheetIndex >= RetroBlitHW.HW_MAX_SPRITESHEETS)
            {
                return false;
            }

            if (SpriteSheets[spritesheetIndex].texture != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get texture of the given offscreen
        /// </summary>
        /// <param name="spriteSheetIndex">Spritesheet index</param>
        /// <returns>Texture</returns>
        public Texture OffscreenGetTexture(int spriteSheetIndex)
        {
            if (spriteSheetIndex < 0 || spriteSheetIndex >= SpriteSheets.Length)
            {
                return null;
            }

            return SpriteSheets[spriteSheetIndex].texture;
        }

        /// <summary>
        /// Set the current render target to the display, also resets the clipping region to cover the display
        /// </summary>
        public void Onscreen()
        {
            if (mCurrentRenderTexture != mFrontBuffer.Texture)
            {
                Flush(FlushReason.OFFSCREEN_CHANGE);
            }

            mCurrentRenderTexture = mFrontBuffer.Texture;
            RB.ClipReset();

            mRetroBlitAPI.PixelCamera.SetRenderTarget(mCurrentRenderTexture);
        }

        /// <summary>
        /// Get scanline effect info
        /// </summary>
        /// <param name="pixelSize">Size of the game pixels in native display pixel size</param>
        /// <param name="offset">Offset in system texture</param>
        /// <param name="length">Length of scanline in system texture</param>
        public void GetScanlineOffsetLength(float pixelSize, out int offset, out int length)
        {
            if (pixelSize < 1)
            {
                pixelSize = 1;
            }

            offset = length = 0;
            offset = (int)pixelSize;
            length = offset;
        }

        /// <summary>
        /// Setup a sprite sheet
        /// </summary>
        /// <param name="index">Index of the sprite sheet</param>
        /// <param name="size">Spritesheet size</param>
        /// <param name="filename">Filename</param>
        /// <param name="texture">Optional RenderTexture to use</param>
        /// <param name="spriteSize">Sprite size</param>
        /// <param name="checkSpritePack">Check if sprite pack of this filename exists</param>
        /// <returns>True if successful</returns>
        public bool SpriteSheetSetup(int index, Vector2i size, string filename, RenderTexture texture, Vector2i spriteSize, bool checkSpritePack = true)
        {
            if (index < 0 || index >= RetroBlitHW.HW_MAX_SPRITESHEETS)
            {
                return false;
            }

            if (filename != null)
            {
                if (filename.EndsWith(".sp.rb"))
                {
                    Debug.LogError("Do not specify the .sp.rb file extension when loading a sprite pack");
                    return false;
                }
                else if (filename.EndsWith(".png") || filename.EndsWith(".jpg") || filename.EndsWith(".jpeg") || filename.EndsWith(".gif") || filename.EndsWith(".tif") || filename.EndsWith(".tga") || filename.EndsWith(".psd"))
                {
                    Debug.LogError("Do not specify the image file extension when loading a sprite. For example, use \"hero\", instead of \"hero.png\"");
                    return false;
                }
            }

            if (filename == null && texture == null)
            {
                if (size.x <= 0 || size.y <= 0)
                {
                    return false;
                }

                RenderTexture tex = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                if (tex == null)
                {
                    return false;
                }

                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.anisoLevel = 0;
                tex.antiAliasing = 1;

                tex.autoGenerateMips = false;
                tex.depth = 0;
                tex.useMipMap = false;

                tex.Create();

                SpriteSheets[index].texture = tex;
                SpriteSheets[index].textureSize = new Vector2i(tex.width, tex.height);
                SpriteSheets[index].spriteSize = new Vector2i(
                    spriteSize.width > SpriteSheets[index].textureSize.width ? SpriteSheets[index].textureSize.width : spriteSize.width,
                    spriteSize.height > SpriteSheets[index].textureSize.height ? SpriteSheets[index].textureSize.height : spriteSize.height);
                SpriteSheets[index].columns = SpriteSheets[index].textureSize.width / SpriteSheets[index].spriteSize.x;
                SpriteSheets[index].rows = SpriteSheets[index].textureSize.height / SpriteSheets[index].spriteSize.y;
                SpriteSheets[index].needsClear = true;

                // If there is no spritesheet set then set this one as the current one
                if (CurrentSpriteSheetIndex == DUMMY_SPRITESHEET_INDEX)
                {
                    SpriteSheetSet(index);
                }

                return true;
            }
            else if (texture != null)
            {
                SpriteSheets[index].texture = texture;
                SpriteSheets[index].textureSize = new Vector2i(texture.width, texture.height);
                SpriteSheets[index].spriteSize = new Vector2i(
                    spriteSize.width > SpriteSheets[index].textureSize.width ? SpriteSheets[index].textureSize.width : spriteSize.width,
                    spriteSize.height > SpriteSheets[index].textureSize.height ? SpriteSheets[index].textureSize.height : spriteSize.height);
                SpriteSheets[index].columns = SpriteSheets[index].textureSize.width / SpriteSheets[index].spriteSize.x;
                SpriteSheets[index].rows = SpriteSheets[index].textureSize.height / SpriteSheets[index].spriteSize.y;

                // If there is no spritesheet set then set this one as the current one
                if (CurrentSpriteSheetIndex == DUMMY_SPRITESHEET_INDEX)
                {
                    SpriteSheetSet(index);
                }
            }
            else
            {
                if (checkSpritePack)
                {
                    // If a spritepack of this filename exists then load it instead of plain texture
                    string spritePackFileName = filename + ".sp.rb/info";
                    var spritePackExists = Resources.Load<TextAsset>(spritePackFileName);
                    if (spritePackExists != null)
                    {
                        var ret = SpritePackSetup(index, filename, spriteSize);
                        if (!ret)
                        {
                            Debug.LogError("Could not load sprite pack from " + filename + ", please try re-importing the sprite pack file in Unity");
                        }

                        return ret;
                    }
                }

#if RETROBLIT_STANDALONE
                var renderTexture = Resources.Load<RenderTexture>(filename);
                if (renderTexture == null)
                {
                    Debug.LogError("Could not load sprite sheet from " + filename + ", make sure the resource is placed somehwere in Assets/Resources folder");
                    SpriteSheets[index].texture = null;
                    return false;
                }

                renderTexture.filterMode = FilterMode.Point;
                renderTexture.wrapMode = TextureWrapMode.Clamp;
                renderTexture.anisoLevel = 0;
                renderTexture.antiAliasing = 1;
                renderTexture.autoGenerateMips = false;
                renderTexture.depth = 0;
                renderTexture.useMipMap = false;

                SpriteSheets[index].texture = renderTexture;
                SpriteSheets[index].textureSize = new Vector2i(renderTexture.width, renderTexture.height);
                SpriteSheets[index].spriteSize = new Vector2i(
                    spriteSize.width > SpriteSheets[index].textureSize.width ? SpriteSheets[index].textureSize.width : spriteSize.width,
                    spriteSize.height > SpriteSheets[index].textureSize.height ? SpriteSheets[index].textureSize.height : spriteSize.height);
                SpriteSheets[index].columns = SpriteSheets[index].textureSize.width / SpriteSheets[index].spriteSize.x;
                SpriteSheets[index].rows = SpriteSheets[index].textureSize.height / SpriteSheets[index].spriteSize.y;

                // If there is no spritesheet set then set this one as the current one
                if (CurrentSpriteSheetIndex == DUMMY_SPRITESHEET_INDEX)
                {
                    SpriteSheetSet(index);
                }
#else
                var spritesTextureOriginal = Resources.Load<Texture2D>(filename);
                if (spritesTextureOriginal == null)
                {
                    Debug.LogError("Could not load sprite sheet from " + filename + ", make sure the resource is placed somehwere in Assets/Resources folder. If this is a sprite pack please try re-importing it in Unity.");
                    SpriteSheets[index].texture = null;
                    return false;
                }

                RenderTexture newTexture;

                newTexture = new RenderTexture(spritesTextureOriginal.width, spritesTextureOriginal.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);

                newTexture.filterMode = FilterMode.Point;
                newTexture.wrapMode = TextureWrapMode.Clamp;
                newTexture.anisoLevel = 0;
                newTexture.antiAliasing = 1;
                newTexture.autoGenerateMips = false;
                newTexture.depth = 0;
                newTexture.useMipMap = false;

                newTexture.Create();

                if (spritesTextureOriginal != null)
                {
                    var oldActive = RenderTexture.active;
                    RenderTexture.active = newTexture;
                    Graphics.Blit(spritesTextureOriginal, newTexture);
                    RenderTexture.active = oldActive;
                }

                SpriteSheets[index].texture = newTexture;
                SpriteSheets[index].textureSize = new Vector2i(newTexture.width, newTexture.height);
                SpriteSheets[index].spriteSize = new Vector2i(
                    spriteSize.width > SpriteSheets[index].textureSize.width ? SpriteSheets[index].textureSize.width : spriteSize.width,
                    spriteSize.height > SpriteSheets[index].textureSize.height ? SpriteSheets[index].textureSize.height : spriteSize.height);
                SpriteSheets[index].columns = SpriteSheets[index].textureSize.width / SpriteSheets[index].spriteSize.x;
                SpriteSheets[index].rows = SpriteSheets[index].textureSize.height / SpriteSheets[index].spriteSize.y;

                // If there is no spritesheet set then set this one as the current one
                if (CurrentSpriteSheetIndex == DUMMY_SPRITESHEET_INDEX)
                {
                    SpriteSheetSet(index);
                }
#endif
            }

            return true;
        }

        /// <summary>
        /// Gets the RenderTexture that the sprite sheet is using.
        /// </summary>
        /// <param name="index">Sprite sheet slot index</param>
        /// <returns>RenderTexture</returns>
        public RenderTexture SpriteSheetTextureGet(int index)
        {
            if (index < 0 || index >= RetroBlitHW.HW_MAX_SPRITESHEETS)
            {
                return null;
            }

            return SpriteSheets[index].texture;
        }

        /// <summary>
        /// Get PackedSprite for given spriteID
        /// </summary>
        /// <param name="spriteID">PackedSprite ID</param>
        /// <param name="spriteSheetIndex">Sprite pack</param>
        /// <returns>PackedSprite</returns>
        public PackedSprite PackedSpriteGet(int spriteID, int spriteSheetIndex = -1)
        {
            if (spriteSheetIndex == -1)
            {
                spriteSheetIndex = CurrentSpriteSheetIndex;
            }

            if (spriteSheetIndex < 0 || spriteSheetIndex >= RetroBlitHW.HW_MAX_SPRITESHEETS)
            {
                return default(PackedSprite);
            }

            var spritePack = SpriteSheets[spriteSheetIndex].spritePack;
            if (spritePack == null)
            {
                return default(PackedSprite);
            }

            if (!spritePack.sprites.ContainsKey(spriteID))
            {
                return default(PackedSprite);
            }

            return spritePack.sprites[spriteID];
        }

        /// <summary>
        /// Setup a sprite sheet
        /// </summary>
        /// <param name="index">Index of the sprite pack, indices are shared with sprite sheets</param>
        /// <param name="filename">Sprite pack filename</param>
        /// <param name="spriteSize">Sprite size</param>
        /// <returns>True if successful</returns>
        public bool SpritePackSetup(int index, string filename, Vector2i spriteSize)
        {
            if (index < 0 || index >= RetroBlitHW.HW_MAX_SPRITESHEETS)
            {
                return false;
            }

            var spritePack = new SpritePack();
            spritePack.sprites = new Dictionary<int, PackedSprite>();

            string infoFileName = filename + ".sp.rb/info";
            var infoFile = Resources.Load<TextAsset>(infoFileName);
            if (infoFile == null)
            {
                Debug.Log("Could not load sprite pack index, please try reimporting" + filename + ".rb");
                return false;
            }

            try
            {
                var reader = new System.IO.BinaryReader(new System.IO.MemoryStream(infoFile.bytes));

                if (reader.ReadUInt16() != RetroBlit_SP_MAGIC)
                {
                    Debug.Log("Sprite pack index file is invalid! Please try reimporting" + filename + ".rb");
                    return false;
                }

                if (reader.ReadUInt16() != RetroBlit_SP_VERSION)
                {
                    Debug.Log("Sprite pack file version is not supported! Please try reimporting" + filename + ".rb");
                    return false;
                }

                int spriteCount = reader.ReadInt32();
                if (spriteCount < 0 || spriteCount > 200000)
                {
                    Debug.Log("Sprite pack sprite count is invalid! Please try reimporting" + filename + ".rb");
                    return false;
                }

                for (int i = 0; i < spriteCount; i++)
                {
                    int hash = reader.ReadInt32();

                    var size = new Vector2i();
                    size.x = (int)reader.ReadUInt16();
                    size.y = (int)reader.ReadUInt16();

                    var sourceRect = new Rect2i();
                    sourceRect.x = (int)reader.ReadUInt16();
                    sourceRect.y = (int)reader.ReadUInt16();
                    sourceRect.width = (int)reader.ReadUInt16();
                    sourceRect.height = (int)reader.ReadUInt16();

                    var trimOffset = new Vector2i();
                    trimOffset.x = (int)reader.ReadUInt16();
                    trimOffset.y = (int)reader.ReadUInt16();

                    var packedSprite = new PackedSprite(new PackedSpriteID(hash), size, sourceRect, trimOffset);

                    spritePack.sprites.Add(hash, packedSprite);
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Sprite pack index file is invalid! Please try reimporting " + filename + ".rb. Exception: " + e.ToString());
                return false;
            }

            var spriteSheetFilename = filename + ".sp.rb/spritepack";
            if (!SpriteSheetSetup(index, Vector2i.zero, spriteSheetFilename, null, new Vector2i(16, 16), false))
            {
                return false;
            }

            SpriteSheets[index].spritePack = spritePack;
            SpriteSheets[index].spriteSize = spriteSize;

            return true;
        }

        /// <summary>
        /// Delete a sprite sheet
        /// </summary>
        /// <param name="index">Index of the sprite sheet</param>
        /// <returns>True if successful</returns>
        public bool SpriteSheetDelete(int index)
        {
            if (index < 0 || index >= RetroBlitHW.HW_MAX_SPRITESHEETS)
            {
                return false;
            }

            SpriteSheets[index].texture = null;
            SpriteSheets[index].spriteSize = new Vector2i(0, 0);
            SpriteSheets[index].textureSize = new Vector2i(0, 0);
            SpriteSheets[index].columns = 0;
            SpriteSheets[index].rows = 0;
            SpriteSheets[index].spritePack = null;

            return true;
        }

        /// <summary>
        /// Set the current sprite sheet to use
        /// </summary>
        /// <param name="index">Index of the sprite sheet</param>
        public void SpriteSheetSet(int index)
        {
            if (index < 0 || index >= RetroBlitHW.HW_MAX_SPRITESHEETS || SpriteSheets[index].texture == null)
            {
                return;
            }

            // Flush if changing textures
            if (mCurrentBatchSpriteIndex != index)
            {
                Flush(FlushReason.SPRITESHEET_CHANGE);
            }

            CurrentSpriteSheetIndex = index;
            if (CurrentSpriteSheetIndex != DUMMY_SPRITESHEET_INDEX)
            {
                SetCurrentTexture(SpriteSheets[CurrentSpriteSheetIndex].texture, SpriteSheets[CurrentSpriteSheetIndex], false);
            }
            else
            {
                SetCurrentTexture(null, mEmptySpriteSheet, false);
            }
        }

        /// <summary>
        /// Save the current effects state at this point, and move to next front buffer.
        /// </summary>
        public void EffectApplyNow()
        {
            var drawState = StoreState();

            bool wasOnFrontBuffer = false;
            if (mCurrentRenderTexture == mFrontBuffer.Texture)
            {
                wasOnFrontBuffer = true;
            }

            Onscreen();
            CameraSet(Vector2i.zero);
            AlphaSet(255);
            TintColorSet(new Color32(255, 255, 255, 255));
            RB.ClipReset();
            ShaderReset();

            mRetroBlitAPI.Effects.ApplyRenderTimeEffects();

            Flush(FlushReason.EFFECT_APPLY);

            mFrontBuffer.NextBuffer(mRetroBlitAPI);

            RestoreState(drawState);

            if (wasOnFrontBuffer)
            {
                mCurrentRenderTexture = mFrontBuffer.Texture;
            }

            ClearTransparent(mFrontBuffer.Texture);
        }

        /// <summary>
        /// Maximum radius of a circle
        /// </summary>
        /// <param name="center">Center of circle</param>
        /// <returns>Max radius</returns>
        public int MaxCircleRadiusForCenter(Vector2i center)
        {
            int maxEdgeDistance = 0;
            if (center.x > maxEdgeDistance)
            {
                maxEdgeDistance = center.x;
            }

            if (center.y > maxEdgeDistance)
            {
                maxEdgeDistance = center.y;
            }

            if (RB.DisplaySize.width - center.x > maxEdgeDistance)
            {
                maxEdgeDistance = RB.DisplaySize.width - center.x;
            }

            if (RB.DisplaySize.height - center.y > maxEdgeDistance)
            {
                maxEdgeDistance = RB.DisplaySize.height - center.y;
            }

            int maxRadius = (int)Mathf.Sqrt(2 * maxEdgeDistance * maxEdgeDistance) + 1;

            return maxRadius;
        }

        /// <summary>
        /// Get current front buffer
        /// </summary>
        /// <returns>Front buffer</returns>
        public FrontBuffer GetFrontBuffer()
        {
            return mFrontBuffer;
        }

        /// <summary>
        /// Clear the given RenderTexture to a transparent color
        /// </summary>
        /// <param name="texture">Texture to clear</param>
        public void ClearTransparent(RenderTexture texture)
        {
            Color32 clearColor = new Color32(0, 0, 0, 0);

            RenderTexture rt = UnityEngine.RenderTexture.active;
            UnityEngine.RenderTexture.active = texture;
            GL.Clear(true, true, clearColor);
            UnityEngine.RenderTexture.active = rt;

            ResetMesh();
        }

        /// <summary>
        /// Load a shader from the given file
        /// </summary>
        /// <param name="index">Shader index to load into</param>
        /// <param name="filename">Shader filename</param>
        /// <returns>True if successful</returns>
        public bool ShaderSetup(int index, string filename)
        {
            if (index < 0 || index >= RetroBlitHW.HW_MAX_SHADERS)
            {
                return false;
            }

            if (filename == null)
            {
                // No filename indicates the user wants to delete the shader, just set to null and return
                mShaders[index] = null;
                return true;
            }

            var shader = Resources.Load<Shader>(filename);
            if (shader == null)
            {
                Debug.Log("Could not load shader from " + filename + ", make sure the resource is placed somehwere in Assets/Resources folder");
                return false;
            }

            var material = new RetroBlitShader(shader);
            mShaders[index] = material;

            return true;
        }

        /// <summary>
        /// Set the current shader
        /// </summary>
        /// <param name="index">Shader index</param>
        public void ShaderSet(int index)
        {
            if (index < 0 || index >= RetroBlitHW.HW_MAX_SHADERS)
            {
                return;
            }

            if (mShaders[index] == null)
            {
                ShaderReset();
            }
            else
            {
                SetCurrentMaterial(mShaders[index]);
                mCurrentShaderIndex = index;

                if (CurrentSpriteSheetIndex == DUMMY_SPRITESHEET_INDEX)
                {
                    SetCurrentTexture(null, mEmptySpriteSheet, true);
                }
                else
                {
                    SetCurrentTexture(SpriteSheets[CurrentSpriteSheetIndex].texture, SpriteSheets[CurrentSpriteSheetIndex], true);
                }
            }
        }

        /// <summary>
        /// Apply the shader now, by flushing
        /// </summary>
        public void ShaderApplyNow()
        {
            Flush(FlushReason.SHADER_APPLY);
        }

        /// <summary>
        /// Reset the shader to default*
        /// </summary>
        public void ShaderReset()
        {
            SetCurrentMaterial(mDrawMaterialRGB);

            Flush(FlushReason.SHADER_RESET);

            mCurrentShaderIndex = -1;
        }

        /// <summary>
        /// Get the shader Material
        /// </summary>
        /// <param name="index">Shader index</param>
        /// <returns>Material</returns>
        public Material ShaderGetMaterial(int index)
        {
            if (index < 0 || index >= mShaders.Length)
            {
                return null;
            }

            return mShaders[index];
        }

        /// <summary>
        /// Get the shader parameters
        /// </summary>
        /// <param name="shaderIndex">Shader index</param>
        /// <returns>Shader parameters</returns>
        public RetroBlitShader ShaderParameters(int shaderIndex)
        {
            return mShaders[shaderIndex];
        }

        /// <summary>
        /// Get the current display surface
        /// </summary>
        /// <returns>Display surface</returns>
        public Texture DisplaySurfaceGet()
        {
            return mCurrentRenderTexture;
        }

        /// <summary>
        /// Currently used mesh storage
        /// </summary>
        /// <returns>MeshStorage</returns>
        public MeshStorage CurrentMeshStorage()
        {
            return mMeshStorage;
        }

        /// <summary>
        /// Flush vertex buffer to screen
        /// </summary>
        /// <param name="reason">Reason for flushing</param>
        public void Flush(FlushReason reason)
        {
            if (!RenderEnabled)
            {
                ResetMesh();
                return;
            }

            if (mMeshStorage.CurrentIndex == 0)
            {
                // Nothing to flush
                mCurrentBatchSpriteIndex = CurrentSpriteSheetIndex;
                return;
            }

            SetShaderValues();

            if (mMeshStorage.CurrentVertex > 0)
            {
                var mesh = mMeshStorage.ReduceAndUploadMesh();
                if (mesh == null)
                {
                    // Could not get mesh, will not be able to draw, drop vertices instead
                    ResetMesh();
                    mCurrentBatchSpriteIndex = CurrentSpriteSheetIndex;
                    return;
                }

                Graphics.SetRenderTarget(mCurrentRenderTexture);

                GL.PushMatrix();
                GL.LoadPixelMatrix(0, mCurrentRenderTexture.width, mCurrentRenderTexture.height, 0);

                // If we're using a custom shader then apply chosen filters to all offscreen surfaces
                if (mCurrentShaderIndex != -1)
                {
                    RetroBlitShader shader = mShaders[mCurrentShaderIndex];
                    int filterCount = 0;
                    var filters = shader.GetOffscreenFilters(out filterCount);
                    for (int i = 0; i < filterCount; i++)
                    {
                        var tex = OffscreenGetTexture(filters[i].spriteSheetIndex);
                        if (tex != null)
                        {
                            tex.filterMode = filters[i].filterMode;
                        }
                    }
                }

                for (int pass = 0; pass < mCurrentDrawMaterial.passCount; pass++)
                {
                    mFlushInfo[(int)reason].Count++;
                    mCurrentDrawMaterial.SetPass(pass);
                    Graphics.DrawMeshNow(mesh.Mesh, Matrix4x4.identity);
                }

                // Revert any filter changes
                if (mCurrentShaderIndex != -1)
                {
                    RetroBlitShader shader = mShaders[mCurrentShaderIndex];
                    int filterCount = 0;
                    var filters = shader.GetOffscreenFilters(out filterCount);
                    for (int i = 0; i < filterCount; i++)
                    {
                        var tex = OffscreenGetTexture(filters[i].spriteSheetIndex);
                        if (tex != null)
                        {
                            tex.filterMode = FilterMode.Point;
                        }
                    }
                }

                GL.PopMatrix();

                ResetMesh();
            }

            mCurrentBatchSpriteIndex = CurrentSpriteSheetIndex;
        }

        private void SetCurrentSpriteSheetEmpty()
        {
            mEmptySpriteSheet.columns = 1;
            mEmptySpriteSheet.rows = 1;
            mEmptySpriteSheet.spriteSize = new Vector2i(1, 1);

            CurrentSpriteSheet = mEmptySpriteSheet;
        }

        /// <summary>
        /// Draw a straight orthogonal line
        /// </summary>
        /// <param name="p0">Start point</param>
        /// <param name="p1">End point</param>
        /// <param name="color">RGB color</param>
        private void DrawOrthoLine(Vector2i p0, Vector2i p1, Color32 color)
        {
            // Don't need to check for CheckFlush here, all callers of this API should check instead

            // Make sure p0 is before p1
            if (p0.x > p1.x || p0.y > p1.y)
            {
                Vector2i tp = p0;
                p0 = p1;
                p1 = tp;
            }

            p0 -= mCameraPos;
            p1 -= mCameraPos;

            if ((p0.x < mClipRegion.x0 && p1.x < mClipRegion.x0) || (p0.x > mClipRegion.x1 && p1.x > mClipRegion.x1))
            {
                return;
            }

            if ((p0.y < mClipRegion.y0 && p1.y < mClipRegion.y0) || (p0.y > mClipRegion.y1 && p1.y > mClipRegion.y1))
            {
                return;
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            // Draw the line just one triangle, make sure it passes through the middle of the pixel
            // Horizontal
            if (p0.y == p1.y)
            {
                mMeshStorage.Verticies[i + 0].x = p0.x - 0.1f;
                mMeshStorage.Verticies[i + 0].y = p0.y - 0.1f;
                mMeshStorage.Verticies[i + 0].z = 0;

                mMeshStorage.Verticies[i + 1].x = p1.x + 1.1f;
                mMeshStorage.Verticies[i + 1].y = p1.y + 0.5f;
                mMeshStorage.Verticies[i + 1].z = 0;

                mMeshStorage.Verticies[i + 2].x = p0.x - 0.1f;
                mMeshStorage.Verticies[i + 2].y = p0.y + 1.1f;
                mMeshStorage.Verticies[i + 2].z = 0;

                mMeshStorage.Colors[i + 0] = color;
                mMeshStorage.Colors[i + 1] = color;
                mMeshStorage.Colors[i + 2] = color;
            }
            else
            {
                mMeshStorage.Verticies[i + 0].x = p0.x - 0.1f;
                mMeshStorage.Verticies[i + 0].y = p0.y - 0.1f;
                mMeshStorage.Verticies[i + 0].z = 0;

                mMeshStorage.Verticies[i + 1].x = p0.x + 1.1f;
                mMeshStorage.Verticies[i + 1].y = p0.y - 0.1f;
                mMeshStorage.Verticies[i + 1].z = 0;

                mMeshStorage.Verticies[i + 2].x = p1.x + 0.5f;
                mMeshStorage.Verticies[i + 2].y = p1.y + 1.1f;
                mMeshStorage.Verticies[i + 2].z = 0;

                mMeshStorage.Colors[i + 0] = color;
                mMeshStorage.Colors[i + 1] = color;
                mMeshStorage.Colors[i + 2] = color;
            }

            i += 3;

            mMeshStorage.Indices[j++] = i - 3;
            mMeshStorage.Indices[j++] = i - 2;
            mMeshStorage.Indices[j++] = i - 1;

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        private DrawState StoreState()
        {
            DrawState state = new DrawState();

            state.Alpha = AlphaGet();
            state.CameraPos = CameraGet();
            state.Clip = ClipGet();
            state.CurrentRenderTexture = mCurrentRenderTexture;
            state.TintColor = TintColorGet();
            state.CurrentMaterial = mCurrentDrawMaterial;

            return state;
        }

        private void RestoreState(DrawState state)
        {
            AlphaSet(state.Alpha);
            CameraSet(state.CameraPos);
            ClipSet(state.Clip);
            mCurrentRenderTexture = state.CurrentRenderTexture;
            TintColorSet(state.TintColor);
            SetCurrentMaterial(state.CurrentMaterial);
        }

        private void DrawEllipseInternal(Vector2i center, Vector2i radius, Color32 color)
        {
            int horizontalCount;
            bool rotated;
            int count;

            if (radius.x < 128 && radius.y < 128)
            {
                count = GetEllipseLines32(center, radius, out horizontalCount, out rotated);
            }
            else
            {
                count = GetEllipseLines64(center, radius, out horizontalCount, out rotated);
            }

            DrawEllipseLineList(center, count, color, horizontalCount, rotated);
        }

        private void DrawEllipseFillInternal(Vector2i center, Vector2i radius, Color32 color)
        {
            int horizontalCount;
            bool rotated;
            int count;

            // If the ellipse excedees maximum size then draw a filled rect instead
            if (radius.x > MAX_ELLIPSE_RADIUS || radius.y > MAX_ELLIPSE_RADIUS)
            {
                DrawRectFill(new Rect2i(center.x - radius.x, center.y - radius.y, (radius.x * 2) + 1, (radius.y * 2) + 1), color);
                return;
            }

            if (radius.x < 128 && radius.y < 128)
            {
                count = GetEllipseLines32(center, radius, out horizontalCount, out rotated);
            }
            else
            {
                count = GetEllipseLines64(center, radius, out horizontalCount, out rotated);
            }

            DrawEllipseLineListFilled(center, count, color, horizontalCount, rotated);
        }

        private void DrawEllipseFillInverseInternal(Vector2i center, Vector2i radius, Color32 color)
        {
            int horizontalCount;
            bool rotated;
            int count;

            if (radius.x < 128 && radius.y < 128)
            {
                count = GetEllipseLines32(center, radius, out horizontalCount, out rotated);
            }
            else
            {
                count = GetEllipseLines64(center, radius, out horizontalCount, out rotated);
            }

            DrawEllipseLineListInverseFilled(center, radius, count, color, horizontalCount, rotated);
        }

        private int GetEllipseLines32(Vector2i center, Vector2i radius, out int horizontalCount, out bool rotated)
        {
            horizontalCount = 0;
            rotated = false;

            int rx, ry;

            if (radius.x > radius.y)
            {
                rx = radius.y;
                ry = radius.x;
                rotated = true;
            }
            else
            {
                rx = radius.x;
                ry = radius.y;
            }

            if (rx < 0 || ry < 0)
            {
                return 0;
            }

            if (rx > MAX_ELLIPSE_RADIUS || ry > MAX_ELLIPSE_RADIUS)
            {
                return 0;
            }

            int radiusXSq = rx * rx;
            int radiusYSq = ry * ry;
            int x = 0, y = ry;
            int p;
            int px = 0;
            int py = 2 * radiusXSq * y;

            int i = 0;
            int lastY = y;
            mPoints[i].x = x;
            mPoints[i].y = y;
            i++;

            p = radiusYSq - (radiusXSq * ry) + (radiusXSq / 4);

            while (px < py)
            {
                x++;
                px = px + (2 * radiusYSq);

                if (p < 0)
                {
                    p = p + radiusYSq + px;
                }
                else
                {
                    y--;
                    py = py - (2 * radiusXSq);
                    p = p + radiusYSq + px - py;
                }

                if (y != lastY)
                {
                    // Finish off last line
                    mPoints[i].x = x - 1;
                    mPoints[i].y = lastY;
                    i++;

                    // Start new line
                    mPoints[i].x = x;
                    mPoints[i].y = y;
                    i++;
                }

                lastY = y;
            }

            // Finish off last line
            if (i % 2 == 1)
            {
                mPoints[i].x = x;
                mPoints[i].y = mPoints[i - 1].y;
                i++;
            }

            horizontalCount = i;
            int lastX = 0;
            bool firstLoop = true;

            p = ((radiusYSq * ((x + 1) * x)) + 1) + (radiusXSq * (y - 1) * (y - 1)) - (radiusXSq * radiusYSq);

            while (y > 0)
            {
                y--;
                py = py - (2 * radiusXSq);
                if (p > 0)
                {
                    p = p + radiusXSq - py;
                }
                else
                {
                    x++;
                    px = px + (2 * radiusYSq);
                    p = p + radiusXSq - py + px;
                }

                if (firstLoop)
                {
                    lastX = x;
                    mPoints[i].x = x;
                    mPoints[i].y = y;
                    i++;

                    firstLoop = false;
                }

                if (x != lastX)
                {
                    // Finish off last line
                    mPoints[i].x = lastX;
                    mPoints[i].y = y + 1;
                    i++;

                    // Start new line
                    mPoints[i].x = x;
                    mPoints[i].y = y;
                    i++;
                }

                lastX = x;
            }

            // Finish off last line
            if (i % 2 == 1)
            {
                mPoints[i].x = mPoints[i - 1].x;
                mPoints[i].y = y;
                i++;
            }

            return i;
        }

        private int GetEllipseLines64(Vector2i center, Vector2i radius, out int horizontalCount, out bool rotated)
        {
            horizontalCount = 0;
            rotated = false;

            int rx, ry;

            if (radius.x > radius.y)
            {
                rx = radius.y;
                ry = radius.x;
                rotated = true;
            }
            else
            {
                rx = radius.x;
                ry = radius.y;
            }

            if (rx < 0 || ry < 0)
            {
                return 0;
            }

            if (rx > MAX_ELLIPSE_RADIUS || ry > MAX_ELLIPSE_RADIUS)
            {
                return 0;
            }

            long radiusXSq = rx * rx;
            long radiusYSq = ry * ry;
            int x = 0, y = ry;
            long p;
            long px = 0;
            long py = 2 * radiusXSq * y;

            int i = 0;
            int lastY = y;
            mPoints[i].x = x;
            mPoints[i].y = y;
            i++;

            p = radiusYSq - (radiusXSq * ry) + (radiusXSq / 4);

            while (px < py)
            {
                x++;
                px = px + (2 * radiusYSq);

                if (p < 0)
                {
                    p = p + radiusYSq + px;
                }
                else
                {
                    y--;
                    py = py - (2 * radiusXSq);
                    p = p + radiusYSq + px - py;
                }

                if (y != lastY)
                {
                    // Finish off last line
                    mPoints[i].x = x - 1;
                    mPoints[i].y = lastY;
                    i++;

                    // Start new line
                    mPoints[i].x = x;
                    mPoints[i].y = y;
                    i++;
                }

                lastY = y;
            }

            // Finish off last line
            if (i % 2 == 1)
            {
                mPoints[i].x = x;
                mPoints[i].y = mPoints[i - 1].y;
                i++;
            }

            horizontalCount = i;
            int lastX = 0;
            bool firstLoop = true;

            p = ((radiusYSq * ((x + 1) * x)) + 1) + (radiusXSq * (y - 1) * (y - 1)) - (radiusXSq * radiusYSq);

            while (y > 0)
            {
                y--;
                py = py - (2 * radiusXSq);
                if (p > 0)
                {
                    p = p + radiusXSq - py;
                }
                else
                {
                    x++;
                    px = px + (2 * radiusYSq);
                    p = p + radiusXSq - py + px;
                }

                if (firstLoop)
                {
                    lastX = x;
                    mPoints[i].x = x;
                    mPoints[i].y = y;
                    i++;

                    firstLoop = false;
                }

                if (x != lastX)
                {
                    // Finish off last line
                    mPoints[i].x = lastX;
                    mPoints[i].y = y + 1;
                    i++;

                    // Start new line
                    mPoints[i].x = x;
                    mPoints[i].y = y;
                    i++;
                }

                lastX = x;
            }

            // Finish off last line
            if (i % 2 == 1)
            {
                mPoints[i].x = mPoints[i - 1].x;
                mPoints[i].y = y;
                i++;
            }

            return i;
        }

        private void DrawLineStrip(Vector2i[] points, int pointCount, Color32 color)
        {
            if (pointCount < 2 || pointCount > points.Length)
            {
                return;
            }

            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6 * (pointCount + 1))
            {
                Flush(FlushReason.BATCH_FULL);
            }

            int i = 0;
            for (; i < pointCount - 2; i++)
            {
                if (points[i] != points[i + 1])
                {
                    DrawLineWithCaps(points[i], points[i + 1], color, true, false);
                }
            }

            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6)
            {
                Flush(FlushReason.BATCH_FULL);
            }

            i++;
            if (points[i].x != points[0].x || points[i].y != points[0].y)
            {
                DrawLineWithCaps(points[i - 1], points[i], color, true, true);
            }
            else
            {
                DrawLineWithCaps(points[i - 1], points[i], color, true, false);
            }
        }

        private void DrawLineList(Vector2i[] points, int pointCount, Color32 color)
        {
            if (pointCount < 2 || pointCount > points.Length)
            {
                return;
            }

            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 6 * (pointCount + 1))
            {
                Flush(FlushReason.BATCH_FULL);
            }

            int i = 0;
            for (; i < pointCount - 1; i += 2)
            {
                DrawLineWithCaps(points[i], points[i + 1], color, true, false);
            }
        }

        private void DrawEllipseLineList(Vector2i center, int pointCount, Color32 color, int horizontalLastIndex, bool rotated)
        {
            if (pointCount < 2)
            {
                return;
            }

            int cx = center.x;
            int cy = center.y;

            if (pointCount < 2 || pointCount > mPoints.Length)
            {
                return;
            }

            Color32 previousTintColor = TintColorGet();
            TintColorSet(color);

            byte previousAlpha = AlphaGet();
            AlphaSet(color.a);

            Vector2i p1, p2;

            if (!rotated)
            {
                // Check flush
                if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 3 * 3)
                {
                    Flush(FlushReason.BATCH_FULL);
                }

                // Horizontal sides
                p1 = mPoints[0];
                p2 = mPoints[1];
                DrawHorizontalLineNoChecks(cx + p1.x - p2.x, cx + p2.x, cy + p2.y);
                DrawHorizontalLineNoChecks(cx + p1.x - p2.x, cx + p2.x, cy - p2.y);

                int i = 2;
                int chunkEnd;
                int chunkSize = 128;

                while (i < horizontalLastIndex)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > horizontalLastIndex)
                    {
                        chunkEnd = horizontalLastIndex;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 4 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        DrawHorizontalLineNoChecks(cx + p1.x, cx + p2.x, cy + p2.y);
                        DrawHorizontalLineNoChecks(cx - p2.x, cx - p1.x, cy + p2.y);

                        // If there is a line that ends at y == 0, then we want to shorten
                        // the reflect so we don't get a repeat pixel at y == 0 from the horizontal
                        // line above. If the line is a single pixel then skip it altogether
                        if (p1.y == 0)
                        {
                            continue;
                        }
                        else if (p2.y == 0)
                        {
                            p2.y++;
                        }

                        DrawHorizontalLineNoChecks(cx + p1.x, cx + p2.x, cy - p2.y);
                        DrawHorizontalLineNoChecks(cx - p2.x, cx - p1.x, cy - p2.y);
                    }
                }

                while (i < pointCount - 2)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > pointCount - 2)
                    {
                        chunkEnd = pointCount - 2;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 4 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        DrawVerticalLineNoChecks(cx + p2.x, cy + p2.y, cy + p1.y);
                        DrawVerticalLineNoChecks(cx + p2.x, cy - p1.y, cy - p2.y);

                        // If there is a line that ends at y == 0, then we want to shorten
                        // the reflect so we don't get a repeat pixel at y == 0 from the horizontal
                        // line above. If the line is a single pixel then skip it altogether
                        if (p1.x == 0)
                        {
                            continue;
                        }
                        else if (p2.x == 0)
                        {
                            p2.x++;
                        }

                        DrawVerticalLineNoChecks(cx - p2.x, cy + p2.y, cy + p1.y);
                        DrawVerticalLineNoChecks(cx - p2.x, cy - p1.y, cy - p2.y);
                    }
                }

                // Check flush
                if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 3 * 3)
                {
                    Flush(FlushReason.BATCH_FULL);
                }

                // Vertical sides
                if (i <= pointCount - 2)
                {
                    p1 = mPoints[i];
                    p2 = mPoints[i + 1];

                    DrawVerticalLineNoChecks(cx + p1.x, cy - p1.y, cy + p2.y + p1.y);
                    DrawVerticalLineNoChecks(cx - p1.x, cy - p1.y, cy + p2.y + p1.y);
                }
            }
            else
            {
                // Check flush
                if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 3 * 3)
                {
                    Flush(FlushReason.BATCH_FULL);
                }

                // Horizontal sides
                p1 = mPoints[0];
                p2 = mPoints[1];
                DrawVerticalLineNoChecks(cx + p2.y, cy + p1.x - p2.x, cy + p2.x);
                DrawVerticalLineNoChecks(cx - p2.y, cy + p1.x - p2.x, cy + p2.x);

                int i = 2;
                int chunkEnd;
                int chunkSize = 128;

                while (i < horizontalLastIndex)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > horizontalLastIndex)
                    {
                        chunkEnd = horizontalLastIndex;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 4 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        DrawVerticalLineNoChecks(cx + p1.y, cy + p1.x, cy + p2.x);
                        DrawVerticalLineNoChecks(cx - p1.y, cy + p1.x, cy + p2.x);

                        // If there is a line that ends at y == 0, then we want to shorten
                        // the reflect so we don't get a repeat pixel at y == 0 from the horizontal
                        // line above. If the line is a single pixel then skip it altogether
                        if (p1.x == 0)
                        {
                            continue;
                        }
                        else if (p2.x == 0)
                        {
                            p2.x++;
                        }

                        DrawVerticalLineNoChecks(cx + p1.y, cy - p2.x, cy - p1.x);
                        DrawVerticalLineNoChecks(cx - p1.y, cy - p2.x, cy - p1.x);
                    }
                }

                while (i < pointCount - 2)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > pointCount - 2)
                    {
                        chunkEnd = pointCount - 2;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 4 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        DrawHorizontalLineNoChecks(cx + p2.y, cx + p1.y, cy + p1.x);
                        DrawHorizontalLineNoChecks(cx + p2.y, cx + p1.y, cy - p1.x);

                        // If there is a line that ends at y == 0, then we want to shorten
                        // the reflect so we don't get a repeat pixel at y == 0 from the horizontal
                        // line above. If the line is a single pixel then skip it altogether
                        if (p1.y == 0)
                        {
                            continue;
                        }
                        else if (p2.y == 0)
                        {
                            p2.y++;
                        }

                        DrawHorizontalLineNoChecks(cx - p1.y, cx - p2.y, cy + p1.x);
                        DrawHorizontalLineNoChecks(cx - p1.y, cx - p2.y, cy - p1.x);
                    }
                }

                // Check flush
                if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 3 * 3)
                {
                    Flush(FlushReason.BATCH_FULL);
                }

                // Vertical sides
                if (i <= pointCount - 2)
                {
                    p1 = mPoints[i];
                    p2 = mPoints[i + 1];

                    DrawHorizontalLineNoChecks(cx - p1.y, cx + p2.y + p1.y, cy + p1.x);
                    DrawHorizontalLineNoChecks(cx - p1.y, cx + p2.y + p1.y, cy - p1.x);
                }
            }

            TintColorSet(previousTintColor);
            AlphaSet(previousAlpha);
        }

        private void DrawEllipseLineListFilled(Vector2i center, int pointCount, Color32 color, int horizontalLastIndex, bool rotated)
        {
            if (pointCount < 2)
            {
                return;
            }

            int cx = center.x;
            int cy = center.y;

            if (pointCount < 2 || pointCount > mPoints.Length)
            {
                return;
            }

            Color32 previousTintColor = TintColorGet();
            TintColorSet(color);

            byte previousAlpha = AlphaGet();
            AlphaSet(color.a);

            Vector2i p1, p2;

            if (!rotated)
            {
                // Horizontal rect
                int i = 0;
                int w, h;
                int chunkEnd;
                int chunkSize = 128;

                while (i < horizontalLastIndex)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > horizontalLastIndex)
                    {
                        chunkEnd = horizontalLastIndex;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 4 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        DrawHorizontalLineNoChecks(cx - p2.x, cx + p2.x, cy - p1.y);
                        DrawHorizontalLineNoChecks(cx - p2.x, cx + p2.x, cy + p1.y);
                    }
                }

                while (i < pointCount - 2)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > pointCount - 2)
                    {
                        chunkEnd = pointCount - 2;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 4 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        w = (p1.x * 2) + 1;
                        h = p1.y - p2.y + 1;

                        if (h > 1)
                        {
                            DrawRectFillNoChecks(cx - p1.x, cy - p1.y, cx + p1.x + 1, cy - p2.y + 1);
                            DrawRectFillNoChecks(cx - p1.x, cy + p2.y, cx + p1.x + 1, cy + p1.y + 1);
                        }
                        else
                        {
                            DrawHorizontalLineNoChecks(cx - p1.x, cx - p1.x + w - 1, cy - p1.y);
                            DrawHorizontalLineNoChecks(cx - p1.x, cx - p1.x + w - 1, cy + p1.y);
                        }
                    }
                }

                // Check flush
                if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 3 * 3)
                {
                    Flush(FlushReason.BATCH_FULL);
                }

                // Vertical sides
                if (i <= pointCount - 2)
                {
                    p1 = mPoints[i];
                    p2 = mPoints[i + 1];

                    DrawRectFillNoChecks(cx - p1.x, cy - p1.y, cx + p1.x + 1, cy + p1.y + 1);
                }
            }
            else
            {
                int i = 0;
                int w, h;
                int chunkEnd;
                int chunkSize = 128;

                while (i < horizontalLastIndex)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > horizontalLastIndex)
                    {
                        chunkEnd = horizontalLastIndex;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 4 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        DrawVerticalLineNoChecks(cx + p1.y, cy - p2.x, cy + p2.x);
                        DrawVerticalLineNoChecks(cx - p1.y, cy - p2.x, cy + p2.x);
                    }
                }

                while (i < pointCount - 2)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > pointCount - 2)
                    {
                        chunkEnd = pointCount - 2;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 4 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        w = p1.y - p2.y + 1;
                        h = (p2.x * 2) + 1;

                        if (w > 1)
                        {
                            DrawRectFillNoChecks(cx - p1.y, cy - p2.x, cx - p2.y + 1, cy + p2.x + 1);
                            DrawRectFillNoChecks(cx + p2.y, cy - p2.x, cx + p1.y + 1, cy + p2.x + 1);
                        }
                        else
                        {
                            DrawVerticalLineNoChecks(cx - p1.y, cy - p2.x, cy - p2.x + h - 1);
                            DrawVerticalLineNoChecks(cx + p1.y, cy - p2.x, cy - p2.x + h - 1);
                        }
                    }
                }

                // Check flush
                if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 3 * 3)
                {
                    Flush(FlushReason.BATCH_FULL);
                }

                if (i <= pointCount - 2)
                {
                    p1 = mPoints[i];
                    p2 = mPoints[i + 1];

                    DrawRectFillNoChecks(cx - p1.y, cy - p2.x, cx + p1.y + 1, cy + p2.x + 1);
                }
            }

            TintColorSet(previousTintColor);
            AlphaSet(previousAlpha);
        }

        private void DrawEllipseLineListInverseFilled(Vector2i center, Vector2i radius, int pointCount, Color32 color, int horizontalLastIndex, bool rotated)
        {
            if (pointCount < 2)
            {
                return;
            }

            int cx = center.x;
            int cy = center.y;

            if (pointCount < 2 || pointCount > mPoints.Length)
            {
                return;
            }

            Color32 previousTintColor = TintColorGet();
            TintColorSet(color);

            byte previousAlpha = AlphaGet();
            AlphaSet(color.a);

            Vector2i p1, p2;

            if (!rotated)
            {
                // Horizontal rect
                int i = 0;
                int w, h;
                int chunkEnd;
                int chunkSize = 128;

                while (i < horizontalLastIndex)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > horizontalLastIndex)
                    {
                        chunkEnd = horizontalLastIndex;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 8 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        DrawHorizontalLineNoChecks(cx - radius.x, cx - p2.x - 1, cy - p1.y);
                        DrawHorizontalLineNoChecks(cx + p2.x + 1, cx + radius.x, cy - p1.y);

                        DrawHorizontalLineNoChecks(cx - radius.x, cx - p2.x - 1, cy + p1.y);
                        DrawHorizontalLineNoChecks(cx + p2.x + 1, cx + radius.x, cy + p1.y);
                    }
                }

                while (i < pointCount - 2)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > pointCount - 2)
                    {
                        chunkEnd = pointCount - 2;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 8 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        w = (p1.x * 2) + 1;
                        h = p1.y - p2.y + 1;

                        if (h > 1)
                        {
                            DrawRectFillNoChecks(cx - radius.x, cy - p1.y, cx - p1.x, cy - p2.y + 1);
                            DrawRectFillNoChecks(cx + p1.x + 1, cy - p1.y, cx + radius.x + 1, cy - p2.y + 1);

                            DrawRectFillNoChecks(cx - radius.x, cy + p2.y, cx - p1.x, cy + p1.y + 1);
                            DrawRectFillNoChecks(cx + p1.x + 1, cy + p2.y, cx + radius.x + 1, cy + p1.y + 1);
                        }
                        else
                        {
                            DrawHorizontalLineNoChecks(cx - radius.x, cx - p1.x - 1, cy - p1.y);
                            DrawHorizontalLineNoChecks(cx - p1.x + w, cx + radius.x, cy - p1.y);

                            DrawHorizontalLineNoChecks(cx - radius.x, cx - p1.x - 1, cy + p1.y);
                            DrawHorizontalLineNoChecks(cx - p1.x + w, cx + radius.x, cy + p1.y);
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                int w, h;
                int chunkEnd;
                int chunkSize = 128;

                while (i < horizontalLastIndex)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > horizontalLastIndex)
                    {
                        chunkEnd = horizontalLastIndex;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 8 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        DrawVerticalLineNoChecks(cx + p1.y, cy - radius.y, cy - p2.x - 1);
                        DrawVerticalLineNoChecks(cx - p1.y, cy + p2.x + 1, cy + radius.y);

                        DrawVerticalLineNoChecks(cx - p1.y, cy - radius.y, cy - p2.x - 1);
                        DrawVerticalLineNoChecks(cx + p1.y, cy + p2.x + 1, cy + radius.y);
                    }
                }

                while (i < pointCount - 2)
                {
                    chunkEnd = i + chunkSize;
                    if (chunkEnd > pointCount - 2)
                    {
                        chunkEnd = pointCount - 2;
                    }

                    if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < ((chunkEnd - i + 1) * 8 * 3) / 2)
                    {
                        Flush(FlushReason.BATCH_FULL);
                    }

                    for (; i < chunkEnd; i += 2)
                    {
                        p1 = mPoints[i];
                        p2 = mPoints[i + 1];

                        w = p1.y - p2.y + 1;
                        h = (p2.x * 2) + 1;

                        if (w > 1)
                        {
                            DrawRectFillNoChecks(cx - p1.y, cy - radius.y, cx - p2.y + 1, cy - p2.x);
                            DrawRectFillNoChecks(cx - p1.y, cy + p2.x + 1, cx - p2.y + 1, cy + radius.y + 1);

                            DrawRectFillNoChecks(cx + p2.y, cy - radius.y, cx + p1.y + 1, cy - p2.x);
                            DrawRectFillNoChecks(cx + p2.y, cy + p2.x + 1, cx + p1.y + 1, cy + radius.y + 1);
                        }
                        else
                        {
                            DrawVerticalLineNoChecks(cx - p1.y, cy - radius.y, cy - p2.x - 1);
                            DrawVerticalLineNoChecks(cx - p1.y, cy - p2.x + h, cy + radius.y);

                            DrawVerticalLineNoChecks(cx + p1.y, cy - radius.y, cy - p2.x - 1);
                            DrawVerticalLineNoChecks(cx + p1.y, cy - p2.x + h, cy + radius.y);
                        }
                    }
                }
            }

            TintColorSet(previousTintColor);
            AlphaSet(previousAlpha);
        }

        private void DrawConvexPolygon(Vector2i center, Vector3[] points, int pointCount, Color32 color)
        {
            if (pointCount < 2 || pointCount > points.Length)
            {
                return;
            }

            // Fast color multiply
            color.r = (byte)((color.r * mCurrentColor.r) / 255);
            color.g = (byte)((color.g * mCurrentColor.g) / 255);
            color.b = (byte)((color.b * mCurrentColor.b) / 255);
            color.a = (byte)((color.a * mCurrentColor.a) / 255);

            // Check flush
            if (mMeshStorage.MaxVertices - mMeshStorage.CurrentVertex < 1 + (pointCount * 2))
            {
                Flush(FlushReason.BATCH_FULL);
            }

            int i = mMeshStorage.CurrentVertex;
            int j = mMeshStorage.CurrentIndex;
            int ci = i;

            mMeshStorage.Verticies[i] = new Vector3(center.x, center.y, 0);
            mMeshStorage.Colors[i] = color;

            i++;

            for (int k = 0; k < pointCount - 1; k++)
            {
                float x1 = points[k].x;
                float y1 = points[k].y;
                float x2 = points[k + 1].x;
                float y2 = points[k + 1].y;

                mMeshStorage.Verticies[i + 0] = new Vector3(x1, y1, 0);
                mMeshStorage.Verticies[i + 1] = new Vector3(x2, y2, 0);

                mMeshStorage.Colors[i + 0] = color;
                mMeshStorage.Colors[i + 1] = color;

                i += 2;

                mMeshStorage.Indices[j++] = ci;
                mMeshStorage.Indices[j++] = i - 1;
                mMeshStorage.Indices[j++] = i - 2;
            }

            mMeshStorage.CurrentVertex = i;
            mMeshStorage.CurrentIndex = j;
        }

        private void SetCurrentMaterial(Material material)
        {
            if (material == mCurrentDrawMaterial)
            {
                return;
            }

            Flush(FlushReason.SET_MATERIAL);

            mCurrentDrawMaterial = material;
            SetShaderValues();
        }

        private void SetCurrentTexture(Texture texture, SpriteSheet spriteSheet, bool force)
        {
            CurrentSpriteSheet = spriteSheet;

            if (mPreviousTexture == texture && !force)
            {
                return;
            }

            mPreviousTexture = texture;

            if (texture == null)
            {
                return;
            }

            Flush(FlushReason.SET_TEXTURE);

            mCurrentDrawMaterial.SetTexture(mPropIDSpritesTexture, texture);
        }

        private void SetShaderValues()
        {
            if (mCurrentRenderTexture == null)
            {
                return;
            }

            int x = mClipRegion.x0;
            int y = mClipRegion.y0;
            int w = mClipRegion.x1 - mClipRegion.x0 + 1;
            int h = mClipRegion.y1 - mClipRegion.y0 + 1;

            int x0 = x;
            int y0 = mCurrentRenderTexture.height - (y + h);
            int x1 = x0 + w;
            int y1 = y0 + h;

            mCurrentDrawMaterial.SetVector(mPropIDClip, new Vector4(x0, y0, x1, y1));
            mCurrentDrawMaterial.SetVector(mPropIDDisplaySize, new Vector2(mCurrentRenderTexture.width, mCurrentRenderTexture.height));
            SetShaderGlobalTint(Color.white);
        }

        private void SetShaderGlobalTint(Color32 tint)
        {
            mCurrentDrawMaterial.SetColor(mPropIDGlobalTint, tint);
        }

        private void ResetMesh()
        {
            mMeshStorage.CurrentVertex = 0;
            mMeshStorage.CurrentIndex = 0;
        }

        /// <summary>
        /// Generate system texture
        /// </summary>
        /// <returns>True if successful</returns>
        private bool GenerateSystemTexture()
        {
            SystemTexture = new Texture2D(RetroBlitHW.HW_SYSTEM_TEXTURE_WIDTH, RetroBlitHW.HW_SYSTEM_TEXTURE_HEIGHT, TextureFormat.ARGB32, false);
            SystemTexture.filterMode = FilterMode.Point;
            SystemTexture.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color32[RetroBlitHW.HW_SYSTEM_TEXTURE_WIDTH * RetroBlitHW.HW_SYSTEM_TEXTURE_HEIGHT];

            System.Array.Clear(pixels, 0, pixels.Length);

            // Fill the first row of pixels with solid white, we'll use this to render solid color geometry
            for (int i = 0; i < RetroBlitHW.HW_SYSTEM_TEXTURE_WIDTH; i++)
            {
                pixels[i + (pixels.Length - RetroBlitHW.HW_SYSTEM_TEXTURE_WIDTH)] = new Color32(255, 255, 255, 255);
            }

            var oldRandState = Random.state;
            Random.InitState(0xDEAD);

            // Create a noise texture that will be sampled for the noise retroness effect
            for (int x = RetroBlitHW.HW_SYSTEM_TEXTURE_WIDTH - 128; x < RetroBlitHW.HW_SYSTEM_TEXTURE_WIDTH; x++)
            {
                for (int y = RetroBlitHW.HW_SYSTEM_TEXTURE_HEIGHT - 128; y < RetroBlitHW.HW_SYSTEM_TEXTURE_HEIGHT; y++)
                {
                    int i = x + (y * RetroBlitHW.HW_SYSTEM_TEXTURE_WIDTH);
                    byte c = (byte)Random.Range(0, 256);
                    pixels[i] = new Color32(c, c, c, 255);
                }
            }

            Random.state = oldRandState;

            // Create a gradient texture used for scanline effect
            int col = 0;
            for (int x = RetroBlitHW.HW_SYSTEM_TEXTURE_WIDTH - 256; x < RetroBlitHW.HW_SYSTEM_TEXTURE_WIDTH - 128; x++)
            {
                int scanHeight = col;

                int row = scanHeight - 1;
                for (int y = RetroBlitHW.HW_SYSTEM_TEXTURE_HEIGHT - scanHeight; y < RetroBlitHW.HW_SYSTEM_TEXTURE_HEIGHT; y++)
                {
                    int i = x + (y * RetroBlitHW.HW_SYSTEM_TEXTURE_WIDTH);

                    byte c = 255;
                    if (scanHeight > 16)
                    {
                        if (row == 0)
                        {
                            c = 0;
                        }
                        else if (row == 1)
                        {
                            c = 64;
                        }
                        else if (row == 2)
                        {
                            c = 128;
                        }
                        else if (row == 3)
                        {
                            c = 196;
                        }
                    }
                    else if (scanHeight > 8)
                    {
                        if (row == 0)
                        {
                            c = 0;
                        }
                        else if (row == 1)
                        {
                            c = 85;
                        }
                        else if (row == 2)
                        {
                            c = 170;
                        }
                    }
                    else if (scanHeight > 3)
                    {
                        if (row == 0)
                        {
                            c = 0;
                        }
                        else if (row == 1)
                        {
                            c = 128;
                        }
                    }
                    else if (scanHeight > 2)
                    {
                        if (row == 0)
                        {
                            c = 0;
                        }
                    }
                    else if (scanHeight > 1)
                    {
                        if (row == 0)
                        {
                            c = 128;
                        }
                    }

                    pixels[i] = new Color32(c, c, c, 255);

                    row--;
                }

                col++;
            }

            SystemTexture.SetPixels32(pixels);
            SystemTexture.Apply();

            return true;
        }

        /// <summary>
        /// Defines a single sprite sheet
        /// </summary>
        public struct SpriteSheet
        {
            /// <summary>
            /// The texture for the spritesheet
            /// </summary>
            public RenderTexture texture;

            /// <summary>
            /// Size of the texture for quick lookup
            /// </summary>
            public Vector2i textureSize;

            /// <summary>
            /// Size of sprites in the texture
            /// </summary>
            public Vector2i spriteSize;

            /// <summary>
            /// Sprite columns in the texture
            /// </summary>
            public int columns;

            /// <summary>
            /// Sprite rows in the texture
            /// </summary>
            public int rows;

            /// <summary>
            /// Indicates if spritesheet needs clear
            /// </summary>
            public bool needsClear;

            /// <summary>
            /// Sprite pack lookup
            /// </summary>
            public SpritePack spritePack;
        }

        private struct FlushInfo
        {
            /// <summary>
            /// Reason for flushing
            /// </summary>
            public string Reason;

            /// <summary>
            /// Amount flushed
            /// </summary>
            public int Count;
        }

        private struct ClipRegion
        {
            public int x0, y0, x1, y1;
        }

        private struct DebugClipRegion
        {
            public Rect2i region;
            public Color32 color;
        }

        private struct DrawState
        {
            public RenderTexture CurrentRenderTexture;
            public Vector2i CameraPos;
            public byte Alpha;
            public Color32 TintColor;
            public Rect2i Clip;
            public Material CurrentMaterial;
        }

        /// <summary>
        /// Sprite pack
        /// </summary>
        public class SpritePack
        {
            /// <summary>
            /// Sprites dictionary
            /// </summary>
            public Dictionary<int, PackedSprite> sprites;
        }

        /// <summary>
        /// Wrapper for Unity Material, adding an extra API for tracking offscreen texture filters
        /// </summary>
        public class RetroBlitShader : Material
        {
            private FilterModeSetting[] mShaderFilters;
            private int mShaderFiltersCount = 0;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="shader">Shader</param>
            public RetroBlitShader(Shader shader) : base(shader)
            {
                mShaderFilters = new FilterModeSetting[RetroBlitHW.HW_MAX_SPRITESHEETS];
                mShaderFiltersCount = 0;
            }

            /// <summary>
            /// Set texture filter
            /// </summary>
            /// <param name="spriteSheetIndex">Offscreen index</param>
            /// <param name="filterMode">Filter mode</param>
            public void SetSpriteSheetFilter(int spriteSheetIndex, FilterMode filterMode)
            {
                int i;
                for (i = 0; i < mShaderFiltersCount; i++)
                {
                    if (mShaderFilters[i].spriteSheetIndex == spriteSheetIndex)
                    {
                        // Point format, no need to store it, this is the default behaviour
                        if (filterMode == FilterMode.Point)
                        {
                            // Remove by swapping with last element
                            if (i < mShaderFiltersCount - 1)
                            {
                                mShaderFilters[i] = mShaderFilters[mShaderFiltersCount - 1];
                            }

                            mShaderFiltersCount--;
                        }
                        else
                        {
                            mShaderFilters[i].filterMode = filterMode;
                        }

                        break;
                    }
                }

                // Not found, new entry, or no entry if Point filter type
                if (i == mShaderFiltersCount && filterMode != FilterMode.Point)
                {
                    mShaderFilters[i].filterMode = filterMode;
                    mShaderFilters[i].spriteSheetIndex = spriteSheetIndex;
                    mShaderFiltersCount++;
                }
            }

            /// <summary>
            /// Get all offscreen filters
            /// </summary>
            /// <param name="count">Filter count</param>
            /// <returns>Filters</returns>
            public FilterModeSetting[] GetOffscreenFilters(out int count)
            {
                count = mShaderFiltersCount;
                return mShaderFilters;
            }

            /// <summary>
            /// Filter mode setting for particular sprite sheet index
            /// </summary>
            public struct FilterModeSetting
            {
                /// <summary>
                /// Sprite sheet index for this filter
                /// </summary>
                public int spriteSheetIndex;

                /// <summary>
                /// Filter mode
                /// </summary>
                public FilterMode filterMode;
            }
        }

        /// <summary>
        /// A collection of front buffers
        /// </summary>
        public class FrontBuffer
        {
            /// <summary>
            /// Size of front buffers, they are all the same size
            /// </summary>
            public Vector2i Size;

            private List<BufferState> mBuffers = new List<BufferState>();
            private int mCurrentBufferIndex = -1;

            /// <summary>
            /// Get current front buffer texture
            /// </summary>
            public RenderTexture Texture
            {
                get
                {
                    if (mCurrentBufferIndex < 0)
                    {
                        return null;
                    }

                    return mBuffers[mCurrentBufferIndex].tex;
                }
            }

            /// <summary>
            /// Resize all front buffers
            /// </summary>
            /// <param name="size">New size</param>
            /// <param name="api">Reference to RetroBlitAPI</param>
            /// <returns>True if successful</returns>
            public bool Resize(Vector2i size, RetroBlitAPI api)
            {
                if (size.x < 0 || size.y < 0)
                {
                    return false;
                }

                if (mBuffers.Count == 0)
                {
                    var tex = api.Renderer.RenderTextureCreate(size);
                    if (tex != null)
                    {
                        tex.name = "FrontBuffer_0";

                        mBuffers.Add(new BufferState(tex));
                        Size = size;
                        mCurrentBufferIndex = 0;
                        return true;
                    }
                }
                else
                {
                    // Same size, nothing to do
                    if (size == Size)
                    {
                        return true;
                    }

                    // Resize all existing buffers
                    for (int i = 0; i < mBuffers.Count; i++)
                    {
                        bool wasActive = false;
                        if (UnityEngine.RenderTexture.active == mBuffers[i].tex)
                        {
                            UnityEngine.RenderTexture.active = null;
                            wasActive = true;
                        }

                        bool wasCameraRenderTarget = false;
                        if (api.PixelCamera.GetRenderTarget() == mBuffers[i].tex)
                        {
                            api.PixelCamera.SetRenderTarget(null);
                            wasCameraRenderTarget = true;
                        }

                        if (size.x == 0 || size.y == 0)
                        {
                            if (mBuffers[i].tex != null)
                            {
                                mBuffers[i].tex.Release();
                                mBuffers[i].tex = null;
                            }
                        }

                        if (mBuffers[i].tex != null)
                        {
                            // Release existing texture
                            mBuffers[i].tex.Release();
                            mBuffers[i].tex = null;
                        }

                        RenderTexture tex = api.Renderer.RenderTextureCreate(size);
                        if (tex == null)
                        {
                            return false;
                        }

                        tex.name = "FrontBuffer_" + i;

                        mBuffers[i] = new BufferState(tex);

                        if (wasCameraRenderTarget)
                        {
                            api.PixelCamera.SetRenderTarget(mBuffers[i].tex);
                        }

                        if (wasActive)
                        {
                            UnityEngine.RenderTexture.active = mBuffers[i].tex;
                        }
                    }

                    Size = size;
                }

                return true;
            }

            /// <summary>
            /// Get next front buffer texture, one will be created if necessary
            /// </summary>
            /// <param name="api">Reference to the RetroBlitAPI</param>
            /// <returns>Next front buffer texture</returns>
            public bool NextBuffer(RetroBlitAPI api)
            {
                int index = mCurrentBufferIndex + 1;

                if (index >= mBuffers.Count)
                {
                    RenderTexture tex = api.Renderer.RenderTextureCreate(RB.DisplaySize);
                    if (tex == null)
                    {
                        return false;
                    }

                    tex.name = "FrontBuffer_" + index;

                    mBuffers.Add(new BufferState(tex));
                }

                if (mCurrentBufferIndex >= 0)
                {
                    api.Effects.CopyState(ref mBuffers[mCurrentBufferIndex].effectParams);
                }

                mCurrentBufferIndex = index;

                return true;
            }

            /// <summary>
            /// Notify that render frame has ended, this stores the effects state at the end of the frame
            /// for later post-process rendering stage
            /// </summary>
            /// <param name="api">Reference to the RetroBlitAPI</param>
            public void FrameEnd(RetroBlitAPI api)
            {
                if (mCurrentBufferIndex >= 0)
                {
                    api.Effects.CopyState(ref mBuffers[mCurrentBufferIndex].effectParams);
                }
            }

            /// <summary>
            /// Reset back to the first frame buffer
            /// </summary>
            public void Reset()
            {
                if (mBuffers.Count > 0)
                {
                    mCurrentBufferIndex = 0;
                }
                else
                {
                    mCurrentBufferIndex = -1;
                }
            }

            /// <summary>
            /// Get all the frame buffers, and the count of used ones
            /// </summary>
            /// <param name="usedBuffers">Count of currently used buffers in the last frame</param>
            /// <returns>List of all the front buffers</returns>
            public List<BufferState> GetBuffers(out int usedBuffers)
            {
                usedBuffers = mCurrentBufferIndex + 1;
                return mBuffers;
            }

            /// <summary>
            /// Checks if given texture is one of the front buffers
            /// </summary>
            /// <param name="tex">Texture</param>
            /// <returns>True if one of the front buffers</returns>
            public bool TextureIsFrontBuffer(Texture tex)
            {
                for (int i = 0; i < mBuffers.Count; i++)
                {
                    if (mBuffers[i].tex == tex)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Contains information about a single frame buffer
            /// </summary>
            public class BufferState
            {
                /// <summary>
                /// Texture
                /// </summary>
                public RenderTexture tex;

                /// <summary>
                /// Copy of all the effects to be applied to this frame buffer
                /// </summary>
                public RetroBlitEffects.EffectParams[] effectParams;

                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="tex">Texture</param>
                public BufferState(RenderTexture tex)
                {
                    this.tex = tex;
                    this.effectParams = null;
                }
            }
        }

        /// <summary>
        /// Stores current mesh information to be uploaded. Vertices, uvs, colors, and indices.
        /// </summary>
        public class MeshStorage
        {
            /// <summary>
            /// Current vertex to write to
            /// </summary>
            public int CurrentVertex = 0;

            /// <summary>
            /// Current index to write to
            /// </summary>
            public int CurrentIndex = 0;

            /// <summary>
            /// Maximum vertices
            /// </summary>
            public int MaxVertices = MAX_VERTEX_PER_MESH;

            /// <summary>
            /// Maximum indices
            /// </summary>
            public int MaxIndecies = MAX_INDICES_PER_MESH;

            /// <summary>
            /// Vertices
            /// </summary>
            public Vector3[] Verticies;

            /// <summary>
            /// UVs
            /// </summary>
            public Vector2[] Uvs;

            /// <summary>
            /// Colors
            /// </summary>
            public Color32[] Colors;

            /// <summary>
            /// Indices
            /// </summary>
            public int[] Indices;

            private List<MeshDef> MeshBucket = new List<MeshDef>();

            /// <summary>
            /// Constructor
            /// </summary>
            public MeshStorage()
            {
                int maxVerts = 4;
                while (true)
                {
                    MeshBucket.Add(new MeshDef(maxVerts));

                    if (maxVerts >= MAX_VERTEX_PER_MESH)
                    {
                        break;
                    }

                    maxVerts *= 2;

                    if (maxVerts > MAX_VERTEX_PER_MESH)
                    {
                        maxVerts = MAX_VERTEX_PER_MESH;
                    }
                }

                // Instead of allocating its own arrays the MeshStorage can share arrays with the biggest mesh
                var biggestMesh = MeshBucket[MeshBucket.Count - 1];
                biggestMesh.SharedArrays = true;

                Verticies = biggestMesh.Verticies;
                Uvs = biggestMesh.Uvs;
                Colors = biggestMesh.Colors;
                Indices = biggestMesh.Indices;

                MaxVertices = Verticies.Length;
                MaxIndecies = Indices.Length;
            }

            /// <summary>
            /// Upload mesh, try to reduce its size if possible
            /// </summary>
            /// <returns>Uploaded mesh</returns>
            public MeshDef ReduceAndUploadMesh()
            {
                MeshDef meshDef = null;

                for (int i = 0; i < MeshBucket.Count; i++)
                {
                    meshDef = MeshBucket[i];
                    if (meshDef.MaxVerts >= CurrentVertex)
                    {
                        break;
                    }
                }

                if (meshDef != null)
                {
                    if (!meshDef.SharedArrays)
                    {
                        System.Array.Copy(Verticies, meshDef.Verticies, CurrentVertex);
                        System.Array.Copy(Uvs, meshDef.Uvs, CurrentVertex);
                        System.Array.Copy(Colors, meshDef.Colors, CurrentVertex);

                        System.Buffer.BlockCopy(Indices, 0, meshDef.Indices, 0, CurrentIndex * sizeof(int));
                    }

                    System.Array.Clear(meshDef.Indices, CurrentIndex, meshDef.Indices.Length - CurrentIndex);

                    meshDef.LastIndicesLength = CurrentIndex;

                    meshDef.Upload();
                }

                CurrentVertex = 0;
                CurrentIndex = 0;

                return meshDef;
            }

            /// <summary>
            /// Stores mesh information to be uploaded. Vertices, uvs, colors, and indices.
            /// </summary>
            public class MeshDef
            {
                /// <summary>
                /// Unity Mesh that will be uploaded to
                /// </summary>
                public Mesh Mesh = new Mesh();

                /// <summary>
                /// Maximum vertices
                /// </summary>
                public int MaxVerts;

                /// <summary>
                /// Maximum indices
                /// </summary>
                public int MaxIndices;

                /// <summary>
                /// Vertices
                /// </summary>
                public Vector3[] Verticies;

                /// <summary>
                /// UVs
                /// </summary>
                public Vector2[] Uvs;

                /// <summary>
                /// Colors
                /// </summary>
                public Color32[] Colors;

                /// <summary>
                /// Indices
                /// </summary>
                public int[] Indices;

                /// <summary>
                /// Length of last indices array
                /// </summary>
                public int LastIndicesLength = 0;

                /// <summary>
                /// Are the arrays shared
                /// </summary>
                public bool SharedArrays = false;

                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="maxVerts">Maximum vertices supported by this mesh</param>
                public MeshDef(int maxVerts)
                {
                    MaxVerts = maxVerts;
                    MaxIndices = maxVerts / 4 * 6;

                    Verticies = new Vector3[MaxVerts];
                    Uvs = new Vector2[MaxVerts];
                    Colors = new Color32[MaxVerts];
                    Indices = new int[MaxIndices];

                    Mesh.MarkDynamic();

                    // Pre-populate the mesh object at startup so there are no hiccups later
                    Mesh.vertices = Verticies;
                    Mesh.uv = Uvs;
                    Mesh.colors32 = Colors;
                    Mesh.SetIndices(Indices, MeshTopology.Triangles, 0, false);
                    Mesh.UploadMeshData(false);

                    LastIndicesLength = 0;
                }

                /// <summary>
                /// Upload mesh data
                /// </summary>
                public void Upload()
                {
                    // Upload meshes
                    Mesh.vertices = Verticies;
                    Mesh.uv = Uvs;
                    Mesh.colors32 = Colors;
                    Mesh.SetIndices(Indices, MeshTopology.Triangles, 0, false);
                    Mesh.UploadMeshData(false);
                }
            }
        }
    }
}
