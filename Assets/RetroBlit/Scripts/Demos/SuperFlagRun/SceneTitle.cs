namespace RetroBlitDemoSuperFlagRun
{
    using UnityEngine;

    /// <summary>
    /// Title scene
    /// </summary>
    public class SceneTitle : Scene
    {
        private EntityFlag mFlagOne;
        private EntityFlag mFlagTwo;

        /// <summary>
        /// Initialize
        /// </summary>
        /// <returns>True if successful</returns>
        public override bool Initialize()
        {
            base.Initialize();

            mFlagOne = new EntityFlag(new Vector2((RB.SpriteSize().width * 2) - 1, (RB.SpriteSize().height * 3) + 8), RB.PLAYER_ONE);
            mFlagTwo = new EntityFlag(new Vector2(RB.DisplaySize.width - RB.SpriteSize().width - 8, (RB.SpriteSize().height * 3) + 8), RB.PLAYER_TWO);

            return true;
        }

        /// <summary>
        /// Update
        /// </summary>
        public override void Update()
        {
            base.Update();

            float c0 = Mathf.Sin(Time.time * 2) + 1.0f;
            c0 /= 2.0f;
            c0 = (c0 * (255 - 125)) + 125;

            if (!TransitionDone())
            {
                return;
            }

            if (RB.ButtonPressed(RB.BTN_ABXY, RB.PLAYER_ANY) || RB.ButtonPressed(RB.BTN_POINTER_ANY, RB.PLAYER_ANY))
            {
                SceneGame scene = new SceneGame();
                scene.Initialize();
                SuperFlagRun game = (SuperFlagRun)RB.Game;
                game.SwitchScene(scene);

                RB.SoundPlay(SuperFlagRun.SOUND_START_GAME);
            }

            mFlagOne.Update();
            mFlagTwo.Update();
        }

        /// <summary>
        /// Render
        /// </summary>
        public override void Render()
        {
            RB.Clear(new Color32(127, 213, 221, 255));

            RB.CameraReset();

            SuperFlagRun game = (SuperFlagRun)RB.Game;

            if (game.TitleMap == null)
            {
                RB.Print(new Vector2i(2, 2), DemoUtil.IndexToRGB(14), "Failed to load title TMX map.\nPlease try re-importing the map Demos/SuperFlagRun/TitleMap.tmx in Unity");
                return;
            }

            RB.CameraSet(new Vector2i(RB.SpriteSize().width, 0));

            RB.DrawMapLayer(SuperFlagRun.MAP_LAYER_TITLE_SKY);

            DrawScrollingClouds();

            RB.CameraSet(new Vector2i(RB.SpriteSize().width, RB.SpriteSize().height * 12));

            RB.DrawMapLayer(SuperFlagRun.MAP_LAYER_TITLE_DECO);
            RB.DrawMapLayer(SuperFlagRun.MAP_LAYER_TITLE_TERRAIN);

            RB.CameraSet(new Vector2i(RB.SpriteSize().width, -RB.SpriteSize().height * 7));

            // Draw Flags
            mFlagOne.Render();
            mFlagTwo.Render();

            // Draw Players
            int x = (RB.SpriteSize().width * 3) + 8;
            int y = RB.SpriteSize().height * 3;
            RB.DrawSprite(RB.SpriteIndex(0, 2), new Vector2i(x, y), 0);
            RB.DrawSprite(RB.SpriteIndex(0, 3), new Vector2i(x, y + RB.SpriteSize().height), 0);

            x = RB.DisplaySize.width - (RB.SpriteSize().width * 2) - 8;
            RB.DrawSprite(RB.SpriteIndex(5, 2), new Vector2i(x, y), RB.FLIP_H);
            RB.DrawSprite(RB.SpriteIndex(5, 3), new Vector2i(x, y + RB.SpriteSize().height), RB.FLIP_H);

            // Draw Castles
            RB.DrawCopy(new Rect2i(0, 64, 48, 64), new Vector2i(RB.SpriteSize().width * 2, RB.SpriteSize().height * 4));
            RB.DrawCopy(new Rect2i(80, 64, 48, 64), new Vector2i(RB.DisplaySize.width - (RB.SpriteSize().width * 3), RB.SpriteSize().height * 4), 0);

            // Draw Title
            RB.CameraReset();
            RB.SpriteSheetSet(SuperFlagRun.SPRITESHEET_TITLE);
            byte tint = (byte)((Mathf.Sin(Time.time * 2) * 60) + 196);
            RB.TintColorSet(new Color32(tint, tint, tint, 255));
            RB.DrawCopy(new Rect2i(0, 0, 323, 103), new Vector2i((RB.DisplaySize.width / 2) - (323 / 2), (int)(Mathf.Sin(Time.time * 2) * 6) + 15));
            RB.TintColorSet(Color.white);
            RB.SpriteSheetSet(SuperFlagRun.SPRITESHEET_SPRITES);

            // Draw Press Any Button
            string str = "PRESS ANY BUTTON";
            Vector2i textSize = RB.PrintMeasure(SuperFlagRun.GAME_FONT, str);
            RB.Print(SuperFlagRun.GAME_FONT, new Vector2i((RB.DisplaySize.width / 2) - (textSize.width / 2), (int)(RB.DisplaySize.height * 0.55f)), Color.white, str);

            RB.Print(new Vector2i(2, RB.DisplaySize.height - 9), Color.black, "RetroBlit technical demo game");

            // Let base render last so it can overlay the scene
            base.Render();
        }

        private void DrawScrollingClouds()
        {
            SuperFlagRun game = (SuperFlagRun)RB.Game;

            if (game.GameMap == null)
            {
                return;
            }

            int totalMapWidth = game.GameMapSize.width * RB.SpriteSize().width;
            int offset = (int)(Time.time * 25) % totalMapWidth;

            RB.CameraSet(new Vector2i(offset, 0));
            RB.DrawMapLayer(SuperFlagRun.MAP_LAYER_CLOUDS);

            RB.CameraSet(new Vector2i(offset - totalMapWidth, 0));
            RB.DrawMapLayer(SuperFlagRun.MAP_LAYER_CLOUDS);
        }
    }
}
