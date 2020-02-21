namespace RetroBlitDemoReel
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Demo Reel demo program. Shows off a majority of the RetroBlit features.
    /// </summary>
    public class DemoReel : RB.IRetroBlitGame
    {
        private int mCurrentScene = 0;
        private List<SceneDemo> mScenes = new List<SceneDemo>();
        private bool mMusicOn = false;

        private float[] mNextSceneTime = new float[]
                                                       {
                                                       8.0f, 8.0f, // Start
                                                       4.0f, 4.0f, 4.0f, 4.0f, 4.0f, 4.0f, 4.0f, 4.0f, 4.0f, // Text
                                                       2.0f, 2.0f, // Pixel Format
                                                       4.0f, // Sound
                                                       4.0f, // Input
                                                       2.0f, // Scanline
                                                       2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f,
                                                       4.0f, // Wavy present effect
                                                       48.0f // Last, hold
                                                       };

        private int mCurrentTimeIndex = -1;
        private float mNextTimeStop = 0;
        private double mStartTime = 0;

        private bool MUSIC_SYNC_ENABLED = false;

        /// <summary>
        /// Query hardware
        /// </summary>
        /// <returns>Hardware configuration</returns>
        public RB.HardwareSettings QueryHardware()
        {
            var hw = new RB.HardwareSettings();

            hw.DisplaySize = new Vector2i(640, 360);

            return hw;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <returns>True if successful.</returns>
        public bool Initialize()
        {
            if (MUSIC_SYNC_ENABLED)
            {
                Cursor.visible = false;
            }

            RB.MusicSetup(0, "Demos/DemoReel/Music/Starter8bitDub");
            MusicPlay();

            mScenes.Add(new SceneGameLoop());
            mScenes.Add(new SceneDrawing());
            mScenes.Add(new SceneText());
            mScenes.Add(new SceneClipOffscreen());
            mScenes.Add(new SceneTilemap());
            mScenes.Add(new SceneTMXProps());
            mScenes.Add(new SceneInfiniteMap());
            mScenes.Add(new SceneSpriteSheetDraw());
            mScenes.Add(new SceneSpritepack());
            mScenes.Add(new SceneEase());
            mScenes.Add(new SceneShader());
            mScenes.Add(new ScenePixelStyle(RB.PixelStyle.Wide));
            mScenes.Add(new ScenePixelStyle(RB.PixelStyle.Tall));
            mScenes.Add(new SceneSound());
            mScenes.Add(new SceneInput());
            mScenes.Add(new SceneEffects(RB.Effect.Scanlines));
            mScenes.Add(new SceneEffects(RB.Effect.Noise));
            mScenes.Add(new SceneEffects(RB.Effect.Desaturation));
            mScenes.Add(new SceneEffects(RB.Effect.Curvature));
            mScenes.Add(new SceneEffects(RB.Effect.Slide));
            mScenes.Add(new SceneEffects(RB.Effect.Wipe));
            mScenes.Add(new SceneEffects(RB.Effect.Shake));
            mScenes.Add(new SceneEffects(RB.Effect.Zoom));
            mScenes.Add(new SceneEffects(RB.Effect.Rotation));
            mScenes.Add(new SceneEffects(RB.Effect.ColorFade));
            mScenes.Add(new SceneEffects(RB.Effect.ColorTint));
            mScenes.Add(new SceneEffects(RB.Effect.Negative));
            mScenes.Add(new SceneEffects(RB.Effect.Pixelate));
            mScenes.Add(new SceneEffects(RB.Effect.Pinhole));
            mScenes.Add(new SceneEffects(RB.Effect.InvertedPinhole));
            mScenes.Add(new SceneEffects(RB.Effect.Fizzle));
            mScenes.Add(new SceneEffectShader());
            mScenes.Add(new SceneEffectApply());

            mCurrentScene = 0;
            mScenes[mCurrentScene].Enter();

            return true;
        }

        /// <summary>
        /// Update
        /// </summary>
        public void Update()
        {
            if (MUSIC_SYNC_ENABLED)
            {
                if (mCurrentTimeIndex == -1)
                {
                    mStartTime = Time.time + 0.08f;  // + scene flips later, - sooner
                    mNextTimeStop = mNextSceneTime[0];
                    mCurrentTimeIndex = 0;
                }

                if (mCurrentTimeIndex < mNextSceneTime.Length)
                {
                    double curTime = Time.time - mStartTime;
                    if (curTime >= mNextTimeStop)
                    {
                        mCurrentTimeIndex++;
                        mNextTimeStop += mNextSceneTime[mCurrentTimeIndex];
                        NextScene();
                    }
                }
            }

            if (RB.ButtonPressed(RB.BTN_SYSTEM))
            {
                Application.Quit();
            }

            mScenes[mCurrentScene].Update();
        }

        /// <summary>
        /// Render
        /// </summary>
        public void Render()
        {
            mScenes[mCurrentScene].Render();
        }

        /// <summary>
        /// Switch to next scene, wrap around at the end
        /// </summary>
        public void NextScene()
        {
            int newScene = mCurrentScene + 1;
            if (newScene >= mScenes.Count)
            {
                newScene = 0;
            }

            mScenes[mCurrentScene].Exit();
            mCurrentScene = newScene;
            mScenes[mCurrentScene].Enter();
        }

        /// <summary>
        /// Switch to previous scene, wrap around at the start
        /// </summary>
        public void PreviousScene()
        {
            int newScene = mCurrentScene - 1;
            if (newScene < 0)
            {
                newScene = mScenes.Count - 1;
            }

            mScenes[mCurrentScene].Exit();
            mCurrentScene = newScene;
            mScenes[mCurrentScene].Enter();
        }

        /// <summary>
        /// Play music
        /// </summary>
        public void MusicPlay()
        {
            RB.MusicVolumeSet(1);
            RB.MusicPlay(0);
            mMusicOn = true;
        }

        /// <summary>
        /// Stop music
        /// </summary>
        public void MusicStop()
        {
            RB.MusicStop();
            mMusicOn = false;
        }

        /// <summary>
        /// Check if music is playing
        /// </summary>
        /// <returns>True if playing, false otherwise</returns>
        public bool MusicPlaying()
        {
            return mMusicOn;
        }
    }
}
