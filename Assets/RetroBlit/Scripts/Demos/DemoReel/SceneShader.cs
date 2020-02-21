namespace RetroBlitDemoReel
{
    using UnityEngine;

    /// <summary>
    /// Demonstrate tilemaps
    /// </summary>
    public class SceneShader : SceneDemo
    {
        private Vector2 mBouncePos;
        private Vector2 mSpeed = new Vector2(2.0f, 2.0f);
        private Vector2 mVelocity;

        private FastString mCodeStr = new FastString(2048);
        private FastString mShaderStr = new FastString(2048);

        private TMXMap mMap;

        /// <summary>
        /// Initialize
        /// </summary>
        /// <returns>True if successful</returns>
        public override bool Initialize()
        {
            base.Initialize();

            return true;
        }

        /// <summary>
        /// Handle scene entry
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            RB.SpriteSheetSetup(0, "Demos/DemoReel/Sprites", new Vector2i(16, 16));
            RB.SpriteSheetSet(0);

            RB.SpriteSheetSetup(1, "Demos/DemoReel/Ghost", new Vector2i(104, 106));

            mMap = RB.MapLoadTMX("Demos/DemoReel/Tilemap");

            if (mMap != null)
            {
                RB.MapLoadTMXLayer(mMap, "Decoration", 0);
                RB.MapLoadTMXLayer(mMap, "Terrain", 1);

                RB.MapLayerSpriteSheetSet(0, 0);
                RB.MapLayerSpriteSheetSet(1, 0);
            }

            var demo = (DemoReel)RB.Game;

            RB.ShaderSetup(0, "Demos/DemoReel/WavyMaskShader");

            RB.SpriteSheetSetup(2, RB.DisplaySize);
            RB.SpriteSheetSetup(3, RB.DisplaySize);

            mBouncePos = new Vector2(RB.DisplaySize.width * 0.5f, RB.DisplaySize.height * 0.55f);
            mVelocity = mSpeed;
        }

        /// <summary>
        /// Handle scene exit
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            RB.MapClear();
        }

        /// <summary>
        /// Update
        /// </summary>
        public override void Update()
        {
            base.Update();

            mBouncePos += mVelocity;

            if (mBouncePos.x < 4)
            {
                mBouncePos.x = 4;
                mVelocity.x = mSpeed.x;
            }

            if (mBouncePos.y < RB.DisplaySize.height / 2)
            {
                mBouncePos.y = RB.DisplaySize.height / 2;
                mVelocity.y = mSpeed.y;
            }

            if (mBouncePos.x > RB.DisplaySize.width - RB.SpriteSize(1).x - 4)
            {
                mBouncePos.x = RB.DisplaySize.width - RB.SpriteSize(1).x - 4;
                mVelocity.x = -mSpeed.x;
            }

            if (mBouncePos.y > RB.DisplaySize.height - RB.SpriteSize(1).height - 4)
            {
                mBouncePos.y = RB.DisplaySize.height - RB.SpriteSize(1).height - 4;
                mVelocity.y = -mSpeed.y;
            }
        }

        /// <summary>
        /// Render
        /// </summary>
        public override void Render()
        {
            var demo = (DemoReel)RB.Game;

            RB.Clear(DemoUtil.IndexToRGB(1));

            DrawTMX(4, 4);
        }

        private void DrawTMX(int x, int y)
        {
            var demo = (DemoReel)RB.Game;

            if (mMap != null)
            {
                RB.Offscreen(2);

                RB.DrawRectFill(new Rect2i(0, 0, RB.DisplaySize.width, RB.DisplaySize.height), DemoUtil.IndexToRGB(22));

                RB.DrawMapLayer(0);
                RB.DrawMapLayer(1);

                RB.Offscreen(3);
                RB.Clear(new Color32(0, 0, 0, 0));
                RB.SpriteSheetSet(1);
                RB.DrawSprite(0, new Vector2i((int)mBouncePos.x, (int)mBouncePos.y), mVelocity.x > 0 ? RB.FLIP_H : 0);

                RB.Onscreen();

                RB.ShaderSet(0);
                RB.ShaderSpriteSheetTextureSet(0, "Mask", 3);
                RB.ShaderFloatSet(0, "Wave", RB.Ticks / 10.0f);
                RB.ShaderSpriteSheetFilterSet(0, 3, RB.Filter.Linear);

                RB.SpriteSheetSet(2);
                RB.DrawCopy(new Rect2i(0, 0, RB.DisplaySize.width, RB.DisplaySize.height), Vector2i.zero);

                RB.ShaderReset();

                RB.SpriteSheetSet(0);
            }
            else
            {
                RB.Print(new Vector2i(x, y + 250), DemoUtil.IndexToRGB(14), "Failed to load TMX map.\nPlease try re-importing the map Demos/DemoReel/Tilemap.tmx in Unity");
            }

            string shaderName = "WavyMaskShader";

            mFormatStr.Set("@C// Custom shaders can be used for many things, like masking!\n");
            mFormatStr.Append("@MRB@N.ShaderSetup(@L0@N, @S\"Demos/DemoReel/").Append(shaderName).Append("\"@N);\n");
            mFormatStr.Append("\n");
            mFormatStr.Append("@C// Draw a tilemap to one offscreen surface\n");
            mFormatStr.Append("@MRB@N.Offscreen(@L0@N);\n");
            mFormatStr.Append("@MRB@N.DrawRectFill(@Knew @MRect2i@N(@L0@N, @L0@N,\n");
            mFormatStr.Append("   @MRB@N.DisplaySize.width, @MRB@N.DisplaySize.height),\n");
            mFormatStr.Append("   @I22@N);\n");
            mFormatStr.Append("@MRB@N.DrawMapLayer(@L0@N);\n");
            mFormatStr.Append("@MRB@N.DrawMapLayer(@L1@N);\n");
            mFormatStr.Append("\n");
            mFormatStr.Append("@C// Draw a mask to the other offscreen surface\n");
            mFormatStr.Append("@MRB@N.Offscreen(@L1@N);\n");
            mFormatStr.Append("@MRB@N.Clear(@Knew @MColor32@N(@L0@N, @L0@N, @L0@N, @L0@N));\n");
            mFormatStr.Append("@MRB@N.SpriteSheetSet(@L1@N);\n");
            mFormatStr.Append("@MRB@N.DrawSprite(@L0@N, @Knew @MVector2i@N(@L").Append((int)mBouncePos.x).Append("@N, @L").Append((int)mBouncePos.y).Append("@N)").Append(mVelocity.x > 0 ? ", RB.FLIP_H" : string.Empty).Append(");\n");

            mFormatStr.Append("\n");
            mFormatStr.Append("@C// Use a custom shader to combine the two!\n");
            mFormatStr.Append("@MRB@N.Onscreen();\n");
            mFormatStr.Append("@MRB@N.ShaderSet(@L0@N);\n");
            mFormatStr.Append("@MRB@N.ShaderSpriteSheetTextureSet(@L0@N, @S\"Mask\"@N, @L1@N);\n");
            mFormatStr.Append("@MRB@N.ShaderFloatSet(@L0@N, @S\"Wave\"@N, @L").Append(RB.Ticks / 10.0f, 2).Append("f@N);\n");
            mFormatStr.Append("@MRB@N.ShaderSpriteSheetFilterSet(@L0@N, @L3@N, @MRB@N.@MFilter@N.Linear);\n");
            mFormatStr.Append("\n");
            mFormatStr.Append("@MRB@N.DrawCopy(@Knew @MRect2i@N(@L0@N, @L0@N,\n   @MRB@N.DisplaySize.width, @MRB@N.DisplaySize.height),\n   @MVector2i@N.zero);\n");
            mFormatStr.Append("\n");
            mFormatStr.Append("@MRB@N.ShaderReset();\n");
            DemoUtil.HighlightCode(mFormatStr, mCodeStr);

            mFormatStr.Set("@C// This shader multiplies in a mask and applies a wavy effect!\n");
            mFormatStr.Append("@KShader@N \"Unlit/").Append(shaderName).Append("\" {\n");
            mFormatStr.Append("  @KSubShader@N {\n");
            mFormatStr.Append("    @C...\n");
            mFormatStr.Append("    @KPass@N {\n");
            mFormatStr.Append("      @C...\n");
            mFormatStr.Append("      @C/*** Insert custom shader variables here ***/\n");
            mFormatStr.Append("      @Ksampler2D@N Mask;\n");
            mFormatStr.Append("      @Kfloat@N Wave;\n");
            mFormatStr.Append("\n");
            mFormatStr.Append("      @Nfrag_in vert(vert_in v, @Kout float4@N screen_pos : @MSV_POSITION@N) {\n");
            mFormatStr.Append("        @C...@N\n");
            mFormatStr.Append("      }\n");
            mFormatStr.Append("\n");
            mFormatStr.Append("      @Kfloat4@N frag(frag_in i, @MUNITY_VPOS_TYPE@N screen_pos : @MVPOS@N) : @MSV_Target@N {\n");
            mFormatStr.Append("        @C...\n");
            mFormatStr.Append("        @C/*** Insert custom fragment shader code here ***/@N\n");
            mFormatStr.Append("        @C// Sample the mask texture@N\n");
            mFormatStr.Append("        i.uv.x += sin(Wave + i.uv.y * @L8@N) * @L0.025@N;\n");
            mFormatStr.Append("        i.uv.y += cos(Wave - i.uv.x * @L8@N) * @L0.015@N;\n");
            mFormatStr.Append("        @Kfloat4@N mask_color = @Mtex2D@N(Mask, i.uv).rgba;\n");
            mFormatStr.Append("\n");
            mFormatStr.Append("        @C// Multiply the sprite pixel by mask color@N\n");
            mFormatStr.Append("        @Kreturn@N sprite_pixel_color * mask_color;\n");
            mFormatStr.Append("      }\n");
            mFormatStr.Append("    }\n");
            mFormatStr.Append("  }\n");
            mFormatStr.Append("}\n");
            DemoUtil.HighlightCode(mFormatStr, mShaderStr);

            RB.Print(new Vector2i(x, y), DemoUtil.IndexToRGB(5), mCodeStr);
            RB.Print(new Vector2i(x + 300, y), DemoUtil.IndexToRGB(5), mShaderStr);
        }
    }
}
