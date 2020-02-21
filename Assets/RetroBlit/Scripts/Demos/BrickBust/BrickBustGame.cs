namespace RetroBlitDemoBrickBust
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Your game! You can of course rename this class to whatever you'd like.
    /// </summary>
    public class BrickBustGame : RB.IRetroBlitGame
    {
        private GameState mState = GameState.NONE;
        private GameState mPendingState = GameState.MAIN_MENU;
        private float mStateChange = 0.0f;

        private MainMenu mMainMenu;
        private GameLevel mLevel;
        private float mShake;

        /// <summary>
        /// Game state
        /// </summary>
        public enum GameState
        {
            /// <summary>
            /// No defined state
            /// </summary>
            NONE,

            /// <summary>
            /// At main menu
            /// </summary>
            MAIN_MENU,

            /// <summary>
            /// In game level
            /// </summary>
            LEVEL
        }

        /// <summary>
        /// Current level
        /// </summary>
        public GameLevel Level
        {
            get { return mLevel; }
        }

        /// <summary>
        /// Query hardware. Here you initialize your retro game hardware.
        /// </summary>
        /// <returns>Hardware settings</returns>
        public RB.HardwareSettings QueryHardware()
        {
            var hw = new RB.HardwareSettings();

            int height = 1920;
            if (!UnityEngine.Application.isMobilePlatform)
            {
                height = 1800;
            }

            // Set your display size
            hw.DisplaySize = new Vector2i(1080 / 5, height / 5); // 216 x 360 (216 x 384 mobile)

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
            RB.SpriteSheetSetup(0, "Demos/BrickBust/Sprites", new Vector2i(10, 10));
            RB.SpriteSheetSet(0);

            mMainMenu = new MainMenu();

            RB.EffectSet(RB.Effect.Scanlines, 0.2f);
            RB.EffectSet(RB.Effect.Noise, 0.01f);

            RB.ShaderSetup(C.SHADER_SHADOW, "Demos/BrickBust/DrawShaderShadow");
            RB.ShaderSpriteSheetTextureSet(C.SHADER_SHADOW, "_SpritesTexture", 0);

            RB.SoundSetup(C.SOUND_HIT_BRICK, "Demos/BrickBust/Sounds/hit");
            RB.SoundSetup(C.SOUND_HIT_WALL, "Demos/BrickBust/Sounds/hit2");
            RB.SoundSetup(C.SOUND_EXPLODE, "Demos/BrickBust/Sounds/explode");
            RB.SoundSetup(C.SOUND_DEATH, "Demos/BrickBust/Sounds/death");
            RB.SoundSetup(C.SOUND_START, "Demos/BrickBust/Sounds/start");
            RB.SoundSetup(C.SOUND_POWERUP, "Demos/BrickBust/Sounds/powerup");
            RB.SoundSetup(C.SOUND_LASERSHOT, "Demos/BrickBust/Sounds/lasershot");
            RB.SoundSetup(C.SOUND_LASERHIT, "Demos/BrickBust/Sounds/laserhit");

            RB.MusicSetup(C.MENU_MUSIC, "Demos/BrickBust/Music/BossFight");
            RB.MusicSetup(C.LEVEL_MUSIC, "Demos/BrickBust/Music/Stage2");

            RB.MusicVolumeSet(0.4f);

            LevelDef.Initialize();

            ChangeState(GameState.MAIN_MENU);

            if (UnityEngine.Application.isMobilePlatform)
            {
                C.ACTION_VERB = "TAP";
            }

            return true;
        }

        /// <summary>
        /// Update, your game logic should live here. Update is called at a fixed interval of 60 times per second.
        /// </summary>
        public void Update()
        {
            if (RB.ButtonPressed(RB.BTN_SYSTEM))
            {
                Application.Quit();
            }

            if (mState == GameState.MAIN_MENU)
            {
                mMainMenu.Update();
            }
            else if (mState == GameState.LEVEL)
            {
                mLevel.Update();
            }

            RB.EffectSet(RB.Effect.Shake, mShake);
            mShake *= 0.85f;

            AnimateStateChange();
        }

        /// <summary>
        /// Render, your drawing code should go here.
        /// </summary>
        public void Render()
        {
            RB.Clear(new Color32(24, 17, 7, 255));

            if (mState == GameState.MAIN_MENU)
            {
                mMainMenu.Render();
            }
            else if (mState == GameState.LEVEL)
            {
                mLevel.Render();
            }
        }

        /// <summary>
        /// Shake screen by given amount
        /// </summary>
        /// <param name="amount">How much to shake</param>
        public void Shake(float amount)
        {
            mShake = Math.Min(amount, 0.4f);
        }

        /// <summary>
        /// Change game state
        /// </summary>
        /// <param name="state">New state</param>
        public void ChangeState(GameState state)
        {
            if (mStateChange > 0.0f && mStateChange < 1.0f)
            {
                return;
            }

            mPendingState = state;

            if (mState != GameState.NONE)
            {
                mStateChange = 1.0f;
                RB.SoundPlay(C.SOUND_START);
            }
            else
            {
                mStateChange = 0.0f;
            }

            if (state != mState)
            {
                if (state == GameState.MAIN_MENU)
                {
                    RB.MusicPlay(C.MENU_MUSIC);
                }
                else if (state == GameState.LEVEL)
                {
                    RB.MusicPlay(C.LEVEL_MUSIC);
                }
            }
        }

        /// <summary>
        /// Animate game state change, allowing for nice transitions between screens
        /// </summary>
        public void AnimateStateChange()
        {
            if (mPendingState != GameState.NONE)
            {
                mStateChange -= 0.025f;
                if (mStateChange < 0.0f)
                {
                    mState = mPendingState;
                    mStateChange = 0.0f;

                    if (mState == GameState.LEVEL)
                    {
                        int levelIndex = 0;
                        if (mLevel != null)
                        {
                            levelIndex = mLevel.Index + 1;
                        }

                        mLevel = new GameLevel(levelIndex);
                    }

                    if (mState != GameState.LEVEL)
                    {
                        mLevel = null;
                    }

                    mPendingState = GameState.NONE;
                }

                RB.EffectSet(RB.Effect.Slide, new Vector2i((int)(Mathf.Sin((1.0f - mStateChange) * Mathf.PI / 2) * (RB.DisplaySize.width + 16)), 0));
            }
            else
            {
                mStateChange += 0.025f;
                if (mStateChange > 1)
                {
                    mStateChange = 1;
                }

                RB.EffectSet(RB.Effect.Slide, new Vector2i((int)(Mathf.Sin((1.0f - mStateChange) * Mathf.PI / 2) * (RB.DisplaySize.width + 16)), 0));
            }
        }
    }
}
