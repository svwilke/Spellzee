namespace RetroBlitDemoRetroDungeoneer
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Your game! You can of course rename this class to whatever you'd like.
    /// </summary>
    public class RetroDungeoneerGame : RB.IRetroBlitGame
    {
        private SceneGame mSceneGame;
        private SceneMainMenu mSceneMainMenu;

        private SceneMessage mSceneMessage;

        private Scene mCurrentScene = null;

        /// <summary>
        /// Get map
        /// </summary>
        public GameMap map
        {
            get
            {
                return mSceneGame.map;
            }
        }

        /// <summary>
        /// Get Flood Map
        /// </summary>
        public FloodMap floodMap
        {
            get
            {
                return mSceneGame.floodMap;
            }
        }

        /// <summary>
        /// Game camera
        /// </summary>
        public GameCamera camera
        {
            get
            {
                return mSceneGame.camera;
            }
        }

        /// <summary>
        /// Player
        /// </summary>
        public EntityID player
        {
            get
            {
                return mSceneGame.player;
            }
        }

        /// <summary>
        /// Query hardware. Here you initialize your retro game hardware.
        /// </summary>
        /// <returns>Hardware settings</returns>
        public RB.HardwareSettings QueryHardware()
        {
            var hw = new RB.HardwareSettings();

            // Set your display size
            hw.DisplaySize = new Vector2i(1280 / 2, 720 / 2);

            // Set tilemap maximum size, default is 256, 256. Keep this close to your minimum required size to save on memory
            //// hw.MapSize = new Vector2i(256, 256);

            // Set tilemap maximum layers, default is 8. Keep this close to your minimum required size to save on memory
            //// hw.MapLayers = 8;

            return hw;
        }

        /// <summary>
        /// Initialize your game here.
        /// </summary>
        /// <returns>Return true if successful</returns>
        public bool Initialize()
        {
            // You can load a spritesheet here
            RB.SpriteSheetSetup(0, "Demos/RetroDungeoneer/SpritePack", new Vector2i(16, 16));
            RB.SpriteSheetSet(0);

            var fontSprite = RB.PackedSpriteGet(S.FONT_RETROBLIT_DROPSHADOW);
            var fontPos = new Vector2i(fontSprite.SourceRect.x + 1, fontSprite.SourceRect.y + 1);
            RB.FontSetup(C.FONT_RETROBLIT_DROPSHADOW, '!', (char)((int)'~' + 8), fontPos, 0, new Vector2i(6, 7), ((int)'~' + 8) - (int)'!' + 1, 1, 1, false);

            RB.MusicSetup(C.MUSIC_MAIN_MENU, "Demos/RetroDungeoneer/Music/ReturnToNowhere");
            RB.MusicSetup(C.MUSIC_GAME, "Demos/RetroDungeoneer/Music/DungeonAmbience");
            RB.MusicSetup(C.MUSIC_DEATH, "Demos/RetroDungeoneer/Music/DeathPiano");
            RB.MusicSetup(C.MUSIC_FOREST, "Demos/RetroDungeoneer/Music/ForestAmbience");

            RB.SoundSetup(C.SOUND_MONSTER_DEATH, "Demos/RetroDungeoneer/Sounds/MonsterDeath");
            RB.SoundSetup(C.SOUND_PLAYER_DEATH, "Demos/RetroDungeoneer/Sounds/PlayerDeath");
            RB.SoundSetup(C.SOUND_FOOT_STEP, "Demos/RetroDungeoneer/Sounds/FootStep");
            RB.SoundSetup(C.SOUND_MONSTER_ATTACK, "Demos/RetroDungeoneer/Sounds/MonsterAttack");
            RB.SoundSetup(C.SOUND_PLAYER_ATTACK, "Demos/RetroDungeoneer/Sounds/PlayerAttack");
            RB.SoundSetup(C.SOUND_INVENTORY, "Demos/RetroDungeoneer/Sounds/Inventory");
            RB.SoundSetup(C.SOUND_DRINK, "Demos/RetroDungeoneer/Sounds/Drink");
            RB.SoundSetup(C.SOUND_MENU_OPEN, "Demos/RetroDungeoneer/Sounds/MenuOpen");
            RB.SoundSetup(C.SOUND_MENU_CLOSE, "Demos/RetroDungeoneer/Sounds/MenuClose");
            RB.SoundSetup(C.SOUND_STAIRS, "Demos/RetroDungeoneer/Sounds/Stairs");
            RB.SoundSetup(C.SOUND_POINTER_SELECT, "Demos/RetroDungeoneer/Sounds/PointerSelect");
            RB.SoundSetup(C.SOUND_SELECT_OPTION, "Demos/RetroDungeoneer/Sounds/SelectOption");
            RB.SoundSetup(C.SOUND_LEVEL_UP, "Demos/RetroDungeoneer/Sounds/LevelUp");
            RB.SoundSetup(C.SOUND_FIREBALL, "Demos/RetroDungeoneer/Sounds/Fireball");
            RB.SoundSetup(C.SOUND_LIGHTNING, "Demos/RetroDungeoneer/Sounds/Lightning");
            RB.SoundSetup(C.SOUND_CONFUSE, "Demos/RetroDungeoneer/Sounds/Confuse");
            RB.SoundSetup(C.SOUND_CHEAT, "Demos/RetroDungeoneer/Sounds/CheatMode");
            RB.SoundSetup(C.SOUND_AGGRO1, "Demos/RetroDungeoneer/Sounds/Aggro1");
            RB.SoundSetup(C.SOUND_AGGRO2, "Demos/RetroDungeoneer/Sounds/Aggro2");
            RB.SoundSetup(C.SOUND_PLAYER_FALL_YELL, "Demos/RetroDungeoneer/Sounds/PlayerFallYell");
            RB.SoundSetup(C.SOUND_PORTAL, "Demos/RetroDungeoneer/Sounds/Portal");
            RB.SoundSetup(C.SOUND_JUMP, "Demos/RetroDungeoneer/Sounds/Jump");
            RB.SoundSetup(C.SOUND_BOW_SHOOT, "Demos/RetroDungeoneer/Sounds/BowShoot");
            RB.SoundSetup(C.SOUND_BOW_HIT, "Demos/RetroDungeoneer/Sounds/BowHit");
            RB.SoundSetup(C.SOUND_WEB, "Demos/RetroDungeoneer/Sounds/Web");
            RB.SoundSetup(C.SOUND_TELEPORT, "Demos/RetroDungeoneer/Sounds/Teleport");
            RB.SoundSetup(C.SOUND_SLIME, "Demos/RetroDungeoneer/Sounds/Slime");

            RB.ShaderSetup(C.SHADER_VIGNETTE, "Demos/RetroDungeoneer/DrawVignette");

            RB.EffectSet(RB.Effect.Scanlines, 0.5f);

            EntityFunctions.Initialize();
            S.InitializeAnims();

            InitializeNewGame.InitializeConstants();

            RenderFunctions.Initialize();

            mSceneGame = new SceneGame();
            mSceneMainMenu = new SceneMainMenu();
            mSceneMessage = new SceneMessage();

            // Generate tile grid, this is a one time operation, we can keep reusing the grid
            var gridColor = Color.white;
            for (int x = 0; x < RB.MapSize.width; x++)
            {
                for (int y = 0; y < RB.MapSize.height; y++)
                {
                    RB.MapSpriteSet(C.LAYER_GRID, new Vector2i(x, y), S.GRID, gridColor);
                }
            }

            ChangeScene(SceneEnum.MAIN_MENU);

            // Collect any garbage created during initilization to avoid a performance hiccup later.
            System.GC.Collect();

            return true;
        }

        /// <summary>
        /// Update, your game logic should live here. Update is called at a fixed interval of 60 times per second.
        /// </summary>
        public void Update()
        {
            // First process message box scene, and if it consumes the update event then
            // don't update any other scene
            if (mSceneMessage.Update())
            {
                return;
            }

            if (mCurrentScene != null)
            {
                mCurrentScene.Update();
            }
        }

        /// <summary>
        /// Render, your drawing code should go here.
        /// </summary>
        public void Render()
        {
            if (mCurrentScene != null)
            {
                mCurrentScene.Render();
            }

            mSceneMessage.Render();
        }

        /// <summary>
        /// Change the current game scene
        /// </summary>
        /// <param name="sceneEnum">Scene enum to change to</param>
        /// <param name="sceneParameters">Scene parameters</param>
        public void ChangeScene(SceneEnum sceneEnum, object sceneParameters = null)
        {
            Scene newScene = null;

            switch (sceneEnum)
            {
                case SceneEnum.MAIN_MENU:
                    newScene = mSceneMainMenu;
                    break;

                case SceneEnum.GAME:
                    newScene = mSceneGame;
                    break;
            }

            if (newScene != null)
            {
                if (mCurrentScene != null)
                {
                    mCurrentScene.Exit();
                }

                newScene.Enter(sceneParameters);

                mCurrentScene = newScene;
            }
        }

        /// <summary>
        /// Show a message box
        /// </summary>
        /// <param name="header">Header text</param>
        /// <param name="message">Message text</param>
        /// <param name="options">Options</param>
        public void ShowMessageBox(FastString header, FastString message, List<SceneMessage.MessageBoxOption> options)
        {
            RB.SoundPlay(C.SOUND_MENU_OPEN, 1, RandomUtils.RandomPitch(0.1f));
            mSceneMessage.ShowMessageBox(header, message, options);
        }

        /// <summary>
        /// Close message box
        /// </summary>
        public void CloseMessageBox()
        {
            RB.SoundPlay(C.SOUND_MENU_CLOSE, 1, RandomUtils.RandomPitch(0.1f));
            mSceneMessage.CloseMessageBox();
        }
    }
}
