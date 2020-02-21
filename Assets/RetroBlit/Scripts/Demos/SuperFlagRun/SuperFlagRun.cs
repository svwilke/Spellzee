namespace RetroBlitDemoSuperFlagRun
{
    using UnityEngine;

    /// <summary>
    /// Simple platformer demo game, with custom physics engine
    /// </summary>
    public class SuperFlagRun : RB.IRetroBlitGame
    {
        /// <summary>
        /// Game font
        /// </summary>
        public const int GAME_FONT = 0;

        /// <summary>
        /// Terrain layer
        /// </summary>
        public const int MAP_LAYER_TERRAIN = 0;

        /// <summary>
        /// Background layer
        /// </summary>
        public const int MAP_LAYER_BACKGROUND = 1;

        /// <summary>
        /// Clouds layer
        /// </summary>
        public const int MAP_LAYER_CLOUDS = 2;

        /// <summary>
        /// Sky layer
        /// </summary>
        public const int MAP_LAYER_SKY = 3;

        /// <summary>
        /// Title terrain layer
        /// </summary>
        public const int MAP_LAYER_TITLE_TERRAIN = 4;

        /// <summary>
        /// Title deco layer
        /// </summary>
        public const int MAP_LAYER_TITLE_DECO = 5;

        /// <summary>
        /// Title deco layer
        /// </summary>
        public const int MAP_LAYER_TITLE_SKY = 6;

        /// <summary>
        /// SpriteSheet sprites
        /// </summary>
        public const int SPRITESHEET_SPRITES = 0;

        /// <summary>
        /// SpriteSheet title
        /// </summary>
        public const int SPRITESHEET_TITLE = 1;

        /// <summary>
        /// SpriteSheet terrain
        /// </summary>
        public const int SPRITESHEET_TERRAIN = 2;

        /// <summary>
        /// SpriteSheet deco
        /// </summary>
        public const int SPRITESHEET_DECO = 3;

        /// <summary>
        /// Jump sound
        /// </summary>
        public const int SOUND_JUMP = 0;

        /// <summary>
        /// Flag pickup sound
        /// </summary>
        public const int SOUND_PICKUP_FLAG = 1;

        /// <summary>
        /// Flag drop sound
        /// </summary>
        public const int SOUND_DROP_FLAG = 2;

        /// <summary>
        /// Start game sound
        /// </summary>
        public const int SOUND_START_GAME = 3;

        /// <summary>
        /// Foot step sound
        /// </summary>
        public const int SOUND_FOOT_STEP = 4;

        private bool mStandalone = true;
        private bool mSinglePlayer = false;

        private Vector2i mGameMapSize;
        private Scene mCurrentScene;
        private Scene mNextScene;

        private TMXMap mGameMap;
        private TMXMap mTitleMap;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="standalone">If true the game will not render to display, used by OldDays demo which handles it's own rendering pass</param>
        /// <param name="singlePlayer">If true the game will run in single player mode</param>
        public SuperFlagRun(bool standalone = true, bool singlePlayer = false)
        {
            mStandalone = standalone;
            mSinglePlayer = singlePlayer;
        }

        /// <summary>
        /// Get the size of the game map
        /// </summary>
        public Vector2i GameMapSize
        {
            get { return mGameMapSize; }
        }

        /// <summary>
        /// Get title map
        /// </summary>
        public TMXMap GameMap
        {
            get { return mGameMap; }
        }

        /// <summary>
        /// Get the game map
        /// </summary>
        public TMXMap TitleMap
        {
            get { return mTitleMap; }
        }

        /// <summary>
        /// Get the current scene
        /// </summary>
        public Scene CurrentScene
        {
            get { return mCurrentScene; }
        }

        /// <summary>
        /// True if single player only
        /// </summary>
        public bool SinglePlayer
        {
            get { return mSinglePlayer; }
        }

        /// <summary>
        /// Query hardware
        /// </summary>
        /// <returns>Hardware settings</returns>
        public RB.HardwareSettings QueryHardware()
        {
            var hw = new RB.HardwareSettings();

            hw.MapSize = new Vector2i(200, 32);
            hw.MapLayers = 7;
            hw.DisplaySize = new Vector2i(480, 270);

            return hw;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <returns>True if successful</returns>
        public bool Initialize()
        {
            RB.SpriteSheetSetup(SPRITESHEET_SPRITES, "Demos/SuperFlagRun/Sprites", new Vector2i(16, 16));
            RB.SpriteSheetSetup(SPRITESHEET_TITLE, "Demos/SuperFlagRun/SpritesTitle", new Vector2i(16, 16));
            RB.SpriteSheetSetup(SPRITESHEET_TERRAIN, "Demos/SuperFlagRun/TilemapTerrain", new Vector2i(16, 16));
            RB.SpriteSheetSetup(SPRITESHEET_DECO, "Demos/SuperFlagRun/TilemapDeco", new Vector2i(16, 16));

            RB.SpriteSheetSet(SPRITESHEET_SPRITES);

            RB.FontSetup(GAME_FONT, 'A', 'Z', new Vector2i(0, 130), 0, new Vector2i(12, 12), 6, 1, -1, false);

            if (!LoadMap())
            {
                return false;
            }

            RB.SoundSetup(SOUND_JUMP, "Demos/SuperFlagRun/Jump");
            RB.SoundSetup(SOUND_PICKUP_FLAG, "Demos/SuperFlagRun/Pickup");
            RB.SoundSetup(SOUND_DROP_FLAG, "Demos/SuperFlagRun/DropFlag");
            RB.SoundSetup(SOUND_START_GAME, "Demos/SuperFlagRun/StartGame");
            RB.SoundSetup(SOUND_FOOT_STEP, "Demos/SuperFlagRun/FootStep");

            RB.MusicSetup(0, "Demos/SuperFlagRun/Music/GoLucky");
            RB.MusicVolumeSet(0.5f);
            RB.MusicPlay(0);

            SceneTitle scene = new SceneTitle();
            scene.Initialize();

            SwitchScene(scene);

            RB.EffectSet(RB.Effect.Scanlines, 0.25f);
            RB.EffectSet(RB.Effect.Noise, 0.05f);

            if (!mStandalone)
            {
                RB.PresentDisable();
            }

            return true;
        }

        /// <summary>
        /// Update
        /// </summary>
        public void Update()
        {
            if (RB.ButtonPressed(RB.BTN_SYSTEM))
            {
                Application.Quit();
            }

            if (mCurrentScene != null)
            {
                mCurrentScene.Update();
            }

            if (mNextScene != null)
            {
                if (mCurrentScene.TransitionDone())
                {
                    mCurrentScene = mNextScene;
                    mCurrentScene.Enter();
                    mNextScene = null;
                }
            }
        }

        /// <summary>
        /// Render
        /// </summary>
        public void Render()
        {
            mCurrentScene.Render();
        }

        /// <summary>
        /// Switch to a new scene
        /// </summary>
        /// <param name="newScene">New scene</param>
        public void SwitchScene(Scene newScene)
        {
            if (mCurrentScene == null)
            {
                mCurrentScene = newScene;
                mCurrentScene.Enter();
            }
            else
            {
                mNextScene = newScene;
                mCurrentScene.Exit();
            }
        }

        private bool LoadMap()
        {
            mTitleMap = RB.MapLoadTMX("Demos/SuperFlagRun/TitleMap");
            mGameMap = RB.MapLoadTMX("Demos/SuperFlagRun/GameMap");

            if (mTitleMap != null)
            {
                RB.MapLoadTMXLayer(mTitleMap, "Terrain", SuperFlagRun.MAP_LAYER_TITLE_TERRAIN);
                RB.MapLoadTMXLayer(mTitleMap, "Deco", SuperFlagRun.MAP_LAYER_TITLE_DECO);
                RB.MapLoadTMXLayer(mTitleMap, "Sky", SuperFlagRun.MAP_LAYER_TITLE_SKY);
            }

            if (mGameMap != null)
            {
                RB.MapLoadTMXLayer(mGameMap, "Sky", SuperFlagRun.MAP_LAYER_SKY);
                RB.MapLoadTMXLayer(mGameMap, "Clouds", SuperFlagRun.MAP_LAYER_CLOUDS);
                RB.MapLoadTMXLayer(mGameMap, "Terrain", SuperFlagRun.MAP_LAYER_TERRAIN);
                RB.MapLoadTMXLayer(mGameMap, "Background", SuperFlagRun.MAP_LAYER_BACKGROUND);
            }

            RB.MapLayerSpriteSheetSet(SuperFlagRun.MAP_LAYER_TITLE_TERRAIN, SPRITESHEET_TERRAIN);
            RB.MapLayerSpriteSheetSet(SuperFlagRun.MAP_LAYER_TITLE_DECO, SPRITESHEET_DECO);
            RB.MapLayerSpriteSheetSet(SuperFlagRun.MAP_LAYER_TITLE_SKY, SPRITESHEET_DECO);

            RB.MapLayerSpriteSheetSet(SuperFlagRun.MAP_LAYER_SKY, SPRITESHEET_DECO);
            RB.MapLayerSpriteSheetSet(SuperFlagRun.MAP_LAYER_CLOUDS, SPRITESHEET_DECO);
            RB.MapLayerSpriteSheetSet(SuperFlagRun.MAP_LAYER_TERRAIN, SPRITESHEET_TERRAIN);
            RB.MapLayerSpriteSheetSet(SuperFlagRun.MAP_LAYER_BACKGROUND, SPRITESHEET_DECO);

            RB.SpriteSheetSet(SuperFlagRun.SPRITESHEET_TERRAIN);

            if (mGameMap != null)
            {
                mGameMapSize = mGameMap.size;

                // Convert TMXProperties to simple ColliderInfo.ColliderType, for faster access
                for (int x = 0; x < mGameMapSize.width; x++)
                {
                    for (int y = 0; y < mGameMapSize.height; y++)
                    {
                        var tilePos = new Vector2i(x, y);
                        var tileProps = RB.MapDataGet<TMXProperties>(MAP_LAYER_TERRAIN, tilePos);
                        if (tileProps != null)
                        {
                            RB.MapDataSet<ColliderInfo.ColliderType>(MAP_LAYER_TERRAIN, tilePos, (ColliderInfo.ColliderType)tileProps.GetInt("ColliderType"));
                        }
                        else
                        {
                            RB.MapDataSet<ColliderInfo.ColliderType>(MAP_LAYER_TERRAIN, tilePos, ColliderInfo.ColliderType.NONE);
                        }
                    }
                }
            }

            return true;
        }
    }
}
