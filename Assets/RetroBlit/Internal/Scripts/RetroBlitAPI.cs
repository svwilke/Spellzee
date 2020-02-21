namespace RetroBlitInternal
{
    using UnityEngine;

    /// <summary>
    /// Internal wrapper class for all the RetroBlit subsystems
    /// </summary>
    public class RetroBlitAPI : MonoBehaviour
    {
        /// <summary>
        /// RetroBlitHW
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public RetroBlitHW HW;

        /// <summary>
        /// RetroBlitRenderer
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public RetroBlitRenderer Renderer;

        /// <summary>
        /// RetroBlitPixelCamera
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public RetroBlitPixelCamera PixelCamera;

        /// <summary>
        /// RetroBlitFont
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public RetroBlitFont Font;

        /// <summary>
        /// RetroBlitTilemap
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public RetroBlitTilemap Tilemap;

        /// <summary>
        /// RetroBlitInput
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public RetroBlitInput Input;

        /// <summary>
        /// RetroBlitAudio
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public RetroBlitAudio Audio;

        /// <summary>
        /// RetroBlitEffects
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public RetroBlitEffects Effects;

        /// <summary>
        /// RetroBlitResourceBucket
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public RetroBlitResourceBucket ResourceBucket;

        /// <summary>
        /// RetroBlitPerf
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public RetroBlitPerf Perf;

        /// <summary>
        /// Ticks
        /// </summary>
#if !RETROBLIT_STANDALONE
        [HideInInspector]
#endif
        public ulong Ticks = 0;

        private bool mInitialized = false;

        /// <summary>
        /// Get initialized state
        /// </summary>
        public bool Initialized
        {
            get { return mInitialized; }
        }

        /// <summary>
        /// Reset ticks
        /// </summary>
        public void TicksReset()
        {
            Ticks = 0;
        }

        /// <summary>
        /// Initialize the subsystem wrapper
        /// </summary>
        /// <param name="settings">Hardware settings to initialize with</param>
        /// <returns>True if successful</returns>
        public bool Initialize(RB.HardwareSettings settings)
        {
            var resourceBucketObj = GameObject.Find("ResourceBucket");
            if (resourceBucketObj == null)
            {
                Debug.Log("Can't find ResourceBucket game object");
                return false;
            }

            ResourceBucket = resourceBucketObj.GetComponent<RetroBlitResourceBucket>();
            if (ResourceBucket == null)
            {
                return false;
            }

            HW = new RetroBlitHW();
            if (HW == null || !HW.Initialize(settings))
            {
                return false;
            }

            var cameraObj = GameObject.Find("RetroBlitPixelCamera");
            if (cameraObj == null)
            {
                Debug.Log("Can't find RetroBlitPixelCamera game object");
                return false;
            }

            PixelCamera = cameraObj.GetComponent<RetroBlitPixelCamera>();
            if (PixelCamera == null || !PixelCamera.Initialize(this))
            {
                return false;
            }

            Renderer = new RetroBlitRenderer();
            if (Renderer == null || !Renderer.Initialize(this))
            {
                return false;
            }

            Font = new RetroBlitFont();
            if (Font == null || !Font.Initialize(this))
            {
                return false;
            }

            Tilemap = new RetroBlitTilemap();
            if (Tilemap == null || !Tilemap.Initialize(this))
            {
                return false;
            }

            Input = new RetroBlitInput();
            if (Input == null || !Input.Initialize(this))
            {
                return false;
            }

            var audioObj = GameObject.Find("RetroBlitAudio");
            if (audioObj == null)
            {
                Debug.Log("Can't find RetroBlitAudio game object");
                return false;
            }

            Audio = audioObj.GetComponent<RetroBlitAudio>();
            if (Audio == null || !Audio.Initialize(this))
            {
                return false;
            }

            Effects = new RetroBlitEffects();
            if (Effects == null || !Effects.Initialize(this))
            {
                return false;
            }

            Perf = new RetroBlitPerf();
            if (Perf == null || !Perf.Initialize(this))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finalize initialization, this is called after the game is initialized
        /// </summary>
        /// <param name="initialized">True if initialized successfully</param>
        public void FinalizeInitialization(bool initialized)
        {
            mInitialized = initialized;
        }

        private void Start()
        {
#if UNITY_EDITOR
            // Debug.Log("Disabling live recompilation");
            UnityEditor.EditorApplication.LockReloadAssemblies();
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            // Debug.Log("Enabling live recompilation");
            UnityEditor.EditorApplication.UnlockReloadAssemblies();
#endif
        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            // Debug.Log("Enabling live recompilation");
            UnityEditor.EditorApplication.UnlockReloadAssemblies();
#endif
        }

        // Input string has to be assembled from Update() because FixedUpdate() will probably drop characters
        private void Update()
        {
            if (!mInitialized)
            {
                return;
            }

            if (Input != null)
            {
                Input.AppendInputString(UnityEngine.Input.inputString);
                Input.UpdateScrollWheel();
            }
        }

        /// <summary>
        /// Heart beat of RetroBlit. RetroBlit runs on a fixed update.
        /// </summary>
        private void FixedUpdate()
        {
            if (!mInitialized)
            {
                return;
            }

            if (Input != null)
            {
                Input.FrameStart();
            }

            var game = RB.Game;
            if (game != null)
            {
                game.Update();
                Ticks++;
            }

            if (Perf != null)
            {
                Perf.UpdateEvent();
            }

            if (Input != null)
            {
                Input.FrameEnd();
            }

            if (Tilemap != null)
            {
                Tilemap.FrameEnd();
            }

            if (Font != null)
            {
                Font.FrameEnd();
            }
        }
    }
}
