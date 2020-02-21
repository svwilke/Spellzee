namespace RetroBlitInternal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Effects subsystem
    /// </summary>
    public class RetroBlitEffects
    {
        /// <summary>
        /// Total count of effects, not including custom shader
        /// </summary>
        public static int TOTAL_EFFECTS = System.Enum.GetNames(typeof(RB.Effect)).Length;

        private EffectParams[] mParams = new EffectParams[System.Enum.GetNames(typeof(RB.Effect)).Length + 2]; // +2 for custom shader and filter type
#pragma warning disable 0414 // Unused warning
        private RetroBlitAPI mRetroBlitAPI = null;
#pragma warning restore 0414

        /// <summary>
        /// Initialize the subsystem
        /// </summary>
        /// <param name="api">Reference to subsystem wrapper</param>
        /// <returns>True if successful</returns>
        public bool Initialize(RetroBlitAPI api)
        {
            mRetroBlitAPI = api;

            for (int i = 0; i < mParams.Length; i++)
            {
                mParams[i] = new EffectParams();
            }

            EffectReset();

            return true;
        }

        /// <summary>
        /// Get parameters for effect
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <returns>Parameters</returns>
        public EffectParams ParamsGet(RB.Effect effect)
        {
            return mParams[(int)effect];
        }

        /// <summary>
        /// Set parameters for effect
        /// </summary>
        /// <param name="type">Effect</param>
        /// <param name="intensity">Intensity</param>
        /// <param name="vec">Vector data</param>
        /// <param name="color">RGB color</param>
        public void EffectSet(RB.Effect type, float intensity, Vector2i vec, Color32 color)
        {
            switch (type)
            {
                case RB.Effect.Noise:
                case RB.Effect.Desaturation:
                case RB.Effect.Curvature:
                case RB.Effect.Shake:
                case RB.Effect.Negative:
                case RB.Effect.Pixelate:
                    ParamsGet(type).Intensity = Mathf.Clamp01(intensity);
                    break;

                case RB.Effect.Scanlines:
                    ParamsGet(type).Intensity = Mathf.Clamp01(intensity);
                    break;

                case RB.Effect.Zoom:
                    ParamsGet(type).Intensity = Mathf.Clamp(intensity, 0, 10000.0f);
                    break;

                case RB.Effect.Slide:
                case RB.Effect.Wipe:
                    ParamsGet(type).Vector = new Vector2i(
                        Mathf.Clamp(vec.x, -RB.DisplaySize.width, RB.DisplaySize.width),
                        Mathf.Clamp(vec.y, -RB.DisplaySize.height, RB.DisplaySize.height));
                    break;

                case RB.Effect.Rotation:
                    ParamsGet(type).Intensity = RetroBlitUtil.WrapAngle(intensity);
                    break;

                case RB.Effect.ColorFade:
                case RB.Effect.ColorTint:
                    ParamsGet(type).Intensity = Mathf.Clamp01(intensity);
                    ParamsGet(type).Color = color;
                    break;

                case RB.Effect.Fizzle:
                    // Increase intensity by 1% to ensure full pixel coverage
                    ParamsGet(type).Intensity = Mathf.Clamp01(intensity) * 1.01f;
                    ParamsGet(type).Color = color;
                    break;

                case RB.Effect.Pinhole:
                case RB.Effect.InvertedPinhole:
                    ParamsGet(type).Intensity = Mathf.Clamp01(intensity);
                    ParamsGet(type).Vector = new Vector2i((int)Mathf.Clamp(vec.x, 0, RB.DisplaySize.width - 1), (int)Mathf.Clamp(vec.y, 0, RB.DisplaySize.height - 1));
                    ParamsGet(type).Color = color;
                    break;
            }
        }

        /// <summary>
        /// Set a custom shader effect
        /// </summary>
        /// <param name="shaderIndex">Shader index to use</param>
        public void EffectShaderSet(int shaderIndex)
        {
            ParamsGet((RB.Effect)TOTAL_EFFECTS).ShaderIndex = shaderIndex;
        }

        /// <summary>
        /// Set texture filter to use with custom shader effect
        /// </summary>
        /// <param name="filterMode">Filter</param>
        public void EffectFilterSet(FilterMode filterMode)
        {
            ParamsGet((RB.Effect)TOTAL_EFFECTS + 1).ShaderIndex = (int)filterMode;
        }

        /// <summary>
        /// Get a copy of the current effect states
        /// </summary>
        /// <param name="paramsCopy">Parameters to copy</param>
        public void CopyState(ref RetroBlitEffects.EffectParams[] paramsCopy)
        {
            if (paramsCopy == null)
            {
                paramsCopy = new EffectParams[mParams.Length];
            }

            for (int i = 0; i < paramsCopy.Length; i++)
            {
                mParams[i].ShallowCopy(ref paramsCopy[i]);
            }
        }

        /// <summary>
        /// Apply render time post processing effects, these are just drawing operations and must
        /// be ran before the other shader-time effects are applied
        /// </summary>
        public void ApplyRenderTimeEffects()
        {
            var renderer = mRetroBlitAPI.Renderer;

            // Pinhole effect
            if (mRetroBlitAPI.Effects.ParamsGet(RB.Effect.Pinhole).Intensity > 0)
            {
                var p = mRetroBlitAPI.Effects.ParamsGet(RB.Effect.Pinhole);

                Vector2i c = new Vector2i((int)(p.Vector.x + 0.5f), (int)(p.Vector.y + 0.5f));

                int r = (int)((1.0f - p.Intensity) * renderer.MaxCircleRadiusForCenter(c));

                renderer.DrawEllipseFill(c, new Vector2i(r, r), p.Color, true);
                if (c.x < RB.DisplaySize.width)
                {
                    renderer.DrawRectFill(new Rect2i(c.x + r + 1, c.y - r, RB.DisplaySize.width - c.x - r - 1, (r * 2) + 1), p.Color, Vector2i.zero);
                }

                if (c.x > 0)
                {
                    renderer.DrawRectFill(new Rect2i(0, c.y - r, c.x - r, (r * 2) + 1), p.Color, Vector2i.zero);
                }

                if (c.y > 0)
                {
                    renderer.DrawRectFill(new Rect2i(0, 0, RB.DisplaySize.width, c.y - r), p.Color, Vector2i.zero);
                }

                if (c.y < RB.DisplaySize.height)
                {
                    renderer.DrawRectFill(new Rect2i(0, c.y + r + 1, RB.DisplaySize.width, RB.DisplaySize.height - (c.y + r + 1)), p.Color, Vector2i.zero);
                }
            }
            else if (mRetroBlitAPI.Effects.ParamsGet(RB.Effect.InvertedPinhole).Intensity > 0)
            {
                var p = mRetroBlitAPI.Effects.ParamsGet(RB.Effect.InvertedPinhole);

                Vector2i c = new Vector2i((int)(p.Vector.x + 0.5f), (int)(p.Vector.y + 0.5f));
                int r = (int)(p.Intensity * renderer.MaxCircleRadiusForCenter(c));

                renderer.DrawEllipseFill(c, new Vector2i(r, r), p.Color, false);
            }

            renderer.DrawClipRegions();
        }

        /// <summary>
        /// Reset all effects back to default/off states
        /// </summary>
        public void EffectReset()
        {
            for (int i = 0; i < mParams.Length; i++)
            {
                mParams[i].Color = Color.white;
                mParams[i].ShaderIndex = 0;
                mParams[i].Intensity = 0.0f;
                mParams[i].Vector = Vector2i.zero;
            }

            mParams[(int)RB.Effect.Zoom].Intensity = 1.0f;
            mParams[TOTAL_EFFECTS].ShaderIndex = -1;
            ParamsGet((RB.Effect)TOTAL_EFFECTS + 1).ShaderIndex = (int)FilterMode.Point;
        }

        /// <summary>
        /// Effect parameters
        /// </summary>
        public class EffectParams
        {
            /// <summary>
            /// Intensity of effect, usually 0.0 to 1.0
            /// </summary>
            public float Intensity;

            /// <summary>
            /// Generic vector data
            /// </summary>
            public Vector2i Vector;

            /// <summary>
            /// RGB color
            /// </summary>
            public Color32 Color;

            /// <summary>
            /// Shader index
            /// </summary>
            public int ShaderIndex;

            /// <summary>
            /// Shallow copy of an effect
            /// </summary>
            /// <param name="paramsCopy">Parameters to copy</param>
            public void ShallowCopy(ref EffectParams paramsCopy)
            {
                if (paramsCopy == null)
                {
                    paramsCopy = new EffectParams();
                }

                paramsCopy.Intensity = Intensity;
                paramsCopy.Vector = Vector;
                paramsCopy.Color = Color;
                paramsCopy.ShaderIndex = ShaderIndex;
            }
        }
    }
}
