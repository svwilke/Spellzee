namespace RetroBlitDemoReel
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Demonstrate post processing effects
    /// </summary>
    public class SceneEffectApply : SceneDemo
    {
        private TMXMap mMap;

        /// <summary>
        /// Handle scene entry
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            RB.SpriteSheetSetup(0, "Demos/DemoReel/Sprites", new Vector2i(16, 16));
            RB.SpriteSheetSet(0);

            RB.EffectReset();

            mMap = RB.MapLoadTMX("Demos/DemoReel/Tilemap");

            if (mMap != null)
            {
                RB.MapLoadTMXLayer(mMap, "Decoration", 0);
                RB.MapLoadTMXLayer(mMap, "Terrain", 1);
            }

            RB.ShaderSetup(0, "Demos/DemoReel/PresentRippleShader");
        }

        /// <summary>
        /// Handle scene exit
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            RB.EffectReset();
            RB.MapClear();
        }

        /// <summary>
        /// Update
        /// </summary>
        public override void Update()
        {
            base.Update();
        }

        /// <summary>
        /// Render
        /// </summary>
        public override void Render()
        {
            var demo = (DemoReel)RB.Game;

            RB.Clear(DemoUtil.IndexToRGB(22));

            int spriteFrame = ((int)RB.Ticks % 40) > 20 ? 1 : 0;

            if (mMap != null)
            {
                RB.DrawMapLayer(0);
                RB.DrawMapLayer(1);
            }

            RB.EffectSet(RB.Effect.Noise, 0.15f);
            RB.EffectSet(RB.Effect.Scanlines, 1.0f);
            RB.EffectSet(RB.Effect.Desaturation, (Mathf.Sin(RB.Ticks / 50.0f) * 0.5f) + 0.5f);

            RB.EffectApplyNow();
            RB.EffectReset();

            if (mMap != null)
            {
                RB.DrawSprite(0 + spriteFrame, new Vector2i(13 * 16, 16 * 16));
            }
            else
            {
                RB.Print(new Vector2i(2, 2), DemoUtil.IndexToRGB(14), "Failed to load TMX map.\nPlease try re-importing the map Demos/DemoReel/Tilemap.tmx in Unity");
            }

            mFormatStr.Set("@C// Specify when post-processing effects should be applied\n");
            mFormatStr.Append("@MRB@N.DrawMapLayer(@L0@N);\n");
            mFormatStr.Append("@MRB@N.DrawMapLayer(@L1@N);\n");
            mFormatStr.Append("\n");
            mFormatStr.Append("@MRB@N.EffectSet(@MRB@N.@MEffect@N.Noise, @L0.15f@N);\n");
            mFormatStr.Append("@MRB@N.EffectSet(@MRB@N.@MEffect@N.Scanlines, @L1.0f@N);\n");
            mFormatStr.Append("@MRB@N.EffectSet(@MRB@N.@MEffect@N.Desaturation, @L").Append((Mathf.Sin(RB.Ticks / 50.0f) * 0.5f) + 0.5f, 2).Append("f@N);\n");
            mFormatStr.Append("\n");
            mFormatStr.Append("@MRB@N.EffectApplyNow();\n");
            mFormatStr.Append("@MRB@N.EffectReset();\n");
            mFormatStr.Append("\n");
            mFormatStr.Append("@MRB@N.DrawSprite(@L").Append(0 + spriteFrame).Append("@N, @Knew@N Vector2i(@L").Append(13 * 16).Append("@N, @L").Append(16 * 16).Append("@N));");

            var size = RB.PrintMeasure(DemoUtil.HighlightCode(mFormatStr, mFinalStr));
            size.x += 4;
            size.y += 4;

            var rect = new Rect2i((RB.DisplaySize.width / 2) - (size.x / 2), (RB.DisplaySize.height / 2) - (size.y / 2), size.x, size.y);
            rect.y -= 64;

            RB.DrawRectFill(rect, DemoUtil.IndexToRGB(1));
            RB.DrawRect(rect, DemoUtil.IndexToRGB(4));
            RB.Print(new Vector2i(rect.x + 2, rect.y + 2), DemoUtil.IndexToRGB(0), mFinalStr);
        }
    }
}
