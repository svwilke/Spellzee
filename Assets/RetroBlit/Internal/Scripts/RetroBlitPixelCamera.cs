namespace RetroBlitInternal
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Pixel Camera subsystem that renders to an offscreen "pixel surface" that has the pixel dimensions of an old videogame console
    /// </summary>
    public sealed class RetroBlitPixelCamera : MonoBehaviour
    {
        private const float SHAKE_INTERVAL = 0.01f;

        private bool mPresentEnabled = true;

        private Material mPresentMaterial;
        private Material mPresentRetroMaterial;
        private Material mPresentRetroNoiseMaterial;
        private Material mCurrentPresentMaterial;

        private UnityEngine.Camera mCamera;

        private Vector2i mPreviousScreenSize = new Vector2i(-1, -1);

        private RetroBlitAPI mRetroBlitAPI = null;

        private Vector2i mPresentSize = Vector2i.zero;
        private Vector2i mPresentOffset = Vector2i.zero;

        private Vector2i mLastShakeOffset = Vector2i.zero;
        private float mShakeDelay = 0;

#if !RETROBLIT_STANDALONE
        private WaitForEndOfFrame mEndOfFrameWait = new WaitForEndOfFrame();
#endif

        private int mPropIDPixelTexture;
        private int mPropIDPixelTextureSize;
        private int mPropIDPresentSize;
        private int mPropIDSystemTexture;
        private int mPropIDSampleFactor;
        private int mPropIDScanlineIntensity;
        private int mPropIDScanlineOffset;
        private int mPropIDScanlineLength;
        private int mPropIDNearestEvenScanHeight;
        private int mPropIDDesaturationIntensity;
        private int mPropIDNoiseIntensity;
        private int mPropIDNoiseSeed;
        private int mPropIDCurvatureIntensity;
        private int mPropIDColorFade;
        private int mPropIDColorFadeIntensity;
        private int mPropIDColorTint;
        private int mPropIDColorTintIntensity;
        private int mPropIDFizzleColor;
        private int mPropIDFizzleIntensity;
        private int mPropIDNegativeIntensity;
        private int mPropIDPixelateIntensity;

        /// <summary>
        /// Initialize subsystem
        /// </summary>
        /// <param name="api">Subsystem wrapper reference</param>
        /// <returns>True if successful</returns>
        public bool Initialize(RetroBlitAPI api)
        {
            mPropIDPixelTexture = Shader.PropertyToID("_PixelTexture");
            mPropIDPixelTextureSize = Shader.PropertyToID("_PixelTextureSize");
            mPropIDPresentSize = Shader.PropertyToID("_PresentSize");
            mPropIDSystemTexture = Shader.PropertyToID("_SystemTexture");
            mPropIDSampleFactor = Shader.PropertyToID("_SampleFactor");
            mPropIDScanlineIntensity = Shader.PropertyToID("_ScanlineIntensity");
            mPropIDScanlineOffset = Shader.PropertyToID("_ScanlineOffset");
            mPropIDScanlineLength = Shader.PropertyToID("_ScanlineLength");
            mPropIDNearestEvenScanHeight = Shader.PropertyToID("_NearestEvenScanHeight");
            mPropIDDesaturationIntensity = Shader.PropertyToID("_DesaturationIntensity");
            mPropIDNoiseIntensity = Shader.PropertyToID("_NoiseIntensity");
            mPropIDNoiseSeed = Shader.PropertyToID("_NoiseSeed");
            mPropIDCurvatureIntensity = Shader.PropertyToID("_CurvatureIntensity");
            mPropIDColorFade = Shader.PropertyToID("_ColorFade");
            mPropIDColorFadeIntensity = Shader.PropertyToID("_ColorFadeIntensity");
            mPropIDColorTint = Shader.PropertyToID("_ColorTint");
            mPropIDColorTintIntensity = Shader.PropertyToID("_ColorTintIntensity");
            mPropIDFizzleColor = Shader.PropertyToID("_FizzleColor");
            mPropIDFizzleIntensity = Shader.PropertyToID("_FizzleIntensity");
            mPropIDNegativeIntensity = Shader.PropertyToID("_NegativeIntensity");
            mPropIDPixelateIntensity = Shader.PropertyToID("_PixelateIntensity");

            mRetroBlitAPI = api;

            mCamera = Camera.main;

            if (RB.DisplaySize.width <= 0 || RB.DisplaySize.height <= 0)
            {
                return false;
            }

            var material = mRetroBlitAPI.ResourceBucket.LoadMaterial("PresentMaterial");
            mPresentMaterial = new Material(material);

            material = mRetroBlitAPI.ResourceBucket.LoadMaterial("PresentRetroMaterial");
            mPresentRetroMaterial = new Material(material);

            material = mRetroBlitAPI.ResourceBucket.LoadMaterial("PresentRetroNoiseMaterial");
            mPresentRetroNoiseMaterial = new Material(material);

            mCurrentPresentMaterial = mPresentMaterial;

            /* Dummy camera exists only to keep Unity Editor IDE from complaining that nothing is rendering,
             * if this is not an editor build then we can destroy this camera */
#if !UNITY_EDITOR
            var dummyCamera = GameObject.Find("RetroBlitDummyCamera");
            if (dummyCamera != null)
            {
                Destroy(dummyCamera);
            }
#endif

            return true;
        }

        /// <summary>
        /// Get Unity camera
        /// </summary>
        /// <returns>Camera</returns>
        public UnityEngine.Camera GetCamera()
        {
            return mCamera;
        }

        /// <summary>
        /// Convert screen point to viewport point
        /// </summary>
        /// <param name="p">Point</param>
        /// <returns>Converted position</returns>
        public Vector3 ScreenToViewportPoint(Vector3 p)
        {
            if (mPresentSize.width < 1 || mPresentSize.height < 1)
            {
                return new Vector3(0, 0, 0);
            }

            p.z = 0;

            p.x -= mPresentOffset.x;
            p.y += mPresentOffset.y;
            p.y = Screen.height - p.y;

            p = mCamera.ScreenToViewportPoint(p);

            p.x /= (Screen.width - (mPresentOffset.x * 2)) / mCamera.pixelRect.width;
            p.y /= (Screen.height - (mPresentOffset.y * 2)) / mCamera.pixelRect.height;

            p.x *= mCamera.pixelRect.width;
            p.y *= mCamera.pixelRect.height;

            return p;
        }

        /// <summary>
        /// Get the current render target
        /// </summary>
        /// <returns>Current render target</returns>
        public RenderTexture GetRenderTarget()
        {
            return mCamera.targetTexture;
        }

        /// <summary>
        /// Set the current render target
        /// </summary>
        /// <param name="renderTarget">Render target</param>
        public void SetRenderTarget(RenderTexture renderTarget)
        {
            mCamera.targetTexture = renderTarget;
            WindowResize();
        }

        /// <summary>
        /// Set presenting to display
        /// </summary>
        /// <param name="enabled">Present if true, don't if false</param>
        public void PresentEnabledSet(bool enabled)
        {
            mPresentEnabled = enabled;
        }

        /// <summary>
        /// Setup all shader global variables, there is a bunch, most are tied to post processing effects
        /// </summary>
        /// <param name="effectParams">Array of all current effects</param>
        /// <param name="pixelTexture">Reference to the pixel texture being rendered</param>
        private void SetShaderGlobals(RetroBlitEffects.EffectParams[] effectParams, RenderTexture pixelTexture)
        {
            var customShader = mRetroBlitAPI.Renderer.ShaderGetMaterial(effectParams[(int)RetroBlitEffects.TOTAL_EFFECTS].ShaderIndex);

            if (customShader != null)
            {
                mCurrentPresentMaterial = customShader;

                mCurrentPresentMaterial.SetTexture(mPropIDPixelTexture, pixelTexture);
                mCurrentPresentMaterial.SetVector(mPropIDPixelTextureSize, new Vector2(RB.DisplaySize.width, RB.DisplaySize.height));
                mCurrentPresentMaterial.SetVector(mPropIDPresentSize, new Vector2(Screen.width, Screen.height));

                FilterMode filterMode = (FilterMode)effectParams[(int)RetroBlitEffects.TOTAL_EFFECTS + 1].ShaderIndex;
                pixelTexture.filterMode = filterMode;
            }
            else
            {
                var scanlineParams = effectParams[(int)RB.Effect.Scanlines];
                var noiseParams = effectParams[(int)RB.Effect.Noise];
                var desatParams = effectParams[(int)RB.Effect.Desaturation];
                var curvParams = effectParams[(int)RB.Effect.Curvature];
                var fizzleParams = effectParams[(int)RB.Effect.Fizzle];
                var zoomParams = effectParams[(int)RB.Effect.Zoom];
                var pixelateParams = effectParams[(int)RB.Effect.Pixelate];
                var colorTintParams = effectParams[(int)RB.Effect.ColorTint];
                var colorFadeParams = effectParams[(int)RB.Effect.ColorFade];
                float negativeIntensity = effectParams[(int)RB.Effect.Negative].Intensity;
                float pixelateIntensity = effectParams[(int)RB.Effect.Pixelate].Intensity;

                if (noiseParams.Intensity > 0 || fizzleParams.Intensity > 0 || scanlineParams.Intensity > 0)
                {
                    mCurrentPresentMaterial = mPresentRetroNoiseMaterial;
                }
                else if (scanlineParams.Intensity > 0 || desatParams.Intensity > 0 || curvParams.Intensity > 0)
                {
                    mCurrentPresentMaterial = mPresentRetroMaterial;
                }
                else
                {
                    mCurrentPresentMaterial = mPresentMaterial;
                }

                mCurrentPresentMaterial.SetTexture(mPropIDPixelTexture, pixelTexture);
                mCurrentPresentMaterial.SetVector(mPropIDPixelTextureSize, new Vector2(RB.DisplaySize.width, RB.DisplaySize.height));
                mCurrentPresentMaterial.SetVector(mPropIDPresentSize, new Vector2(Screen.width, Screen.height));

                if (noiseParams.Intensity > 0 || fizzleParams.Intensity > 0 || scanlineParams.Intensity > 0)
                {
                    mCurrentPresentMaterial.SetTexture(mPropIDSystemTexture, mRetroBlitAPI.Renderer.SystemTexture);
                }

                float sampleFactor = 0;

                if (pixelateParams.Intensity == 0)
                {
                    if (mPresentSize.width % RB.DisplaySize.width != 0 || mPresentSize.height % RB.DisplaySize.height != 0 ||
                        curvParams.Intensity != 0)
                    {
                        sampleFactor = 1.0f / (((float)Screen.width / RB.DisplaySize.width) * 2.5f);
                        if (zoomParams.Intensity != 1)
                        {
                            sampleFactor /= zoomParams.Intensity;
                        }
                    }
                }

                mCurrentPresentMaterial.SetFloat(mPropIDSampleFactor, sampleFactor);

                // Apply retroness
                mCurrentPresentMaterial.SetFloat(mPropIDScanlineIntensity, scanlineParams.Intensity);
                if (scanlineParams.Intensity > 0)
                {
                    int offset;
                    int length;
                    float pixelSize = mPresentSize.height / (float)RB.DisplaySize.height;
                    mRetroBlitAPI.Renderer.GetScanlineOffsetLength(pixelSize, out offset, out length);

                    mCurrentPresentMaterial.SetFloat(mPropIDScanlineOffset, offset);
                    mCurrentPresentMaterial.SetFloat(mPropIDScanlineLength, length);

                    // Scanlines look horrible if the screen is not evenly divisible by pixel size. Pass the scanline shader a
                    // screensize thats the nearest height evenly divisible by pixel size.
                    int nearestEvenScanHeight = 0;
                    if (mPresentSize.height > 0 && RB.DisplaySize.height > 0)
                    {
                        nearestEvenScanHeight = Mathf.FloorToInt(mPresentSize.height / (float)length);
                    }

                    mCurrentPresentMaterial.SetFloat(mPropIDNearestEvenScanHeight, nearestEvenScanHeight);
                }

                mCurrentPresentMaterial.SetFloat(mPropIDDesaturationIntensity, desatParams.Intensity);
                if (noiseParams.Intensity > 0)
                {
                    mCurrentPresentMaterial.SetFloat(mPropIDNoiseIntensity, noiseParams.Intensity);

                    var oldRandState = Random.state;
                    Random.InitState((int)RB.Ticks);

                    mCurrentPresentMaterial.SetVector(mPropIDNoiseSeed, new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));

                    Random.state = oldRandState;
                }
                else
                {
                    mCurrentPresentMaterial.SetFloat(mPropIDNoiseIntensity, 0);
                }

                mCurrentPresentMaterial.SetFloat(mPropIDCurvatureIntensity, curvParams.Intensity);

                // Color Fade
                Color32 colorFade;
                colorFade = colorFadeParams.Color;

                mCurrentPresentMaterial.SetVector(mPropIDColorFade, new Vector3(colorFade.r / 255.0f, colorFade.g / 255.0f, colorFade.b / 255.0f));
                mCurrentPresentMaterial.SetFloat(mPropIDColorFadeIntensity, colorFadeParams.Intensity);

                // Color Tint
                Color32 colorTint;
                colorTint = colorTintParams.Color;

                mCurrentPresentMaterial.SetVector(mPropIDColorTint, new Vector3(colorTint.r / 255.0f, colorTint.g / 255.0f, colorTint.b / 255.0f));
                mCurrentPresentMaterial.SetFloat(mPropIDColorTintIntensity, colorTintParams.Intensity);

                // Fizzle
                Color32 colorFizzle;
                colorFizzle = fizzleParams.Color;

                mCurrentPresentMaterial.SetVector(mPropIDFizzleColor, new Vector3(colorFizzle.r / 255.0f, colorFizzle.g / 255.0f, colorFizzle.b / 255.0f));
                mCurrentPresentMaterial.SetFloat(mPropIDFizzleIntensity, fizzleParams.Intensity);

                mCurrentPresentMaterial.SetFloat(mPropIDNegativeIntensity, negativeIntensity);

                mCurrentPresentMaterial.SetFloat(mPropIDPixelateIntensity, pixelateIntensity);
            }
        }

        private void WindowResize()
        {
            if (mCamera.targetTexture == null)
            {
                return;
            }

            mCamera.orthographicSize = mCamera.targetTexture.height / 2.0f;
            mCamera.rect = new Rect(0, 0, mCamera.targetTexture.width, mCamera.targetTexture.height);
            mCamera.transform.position = new Vector3(0, 0, -10);
            mCamera.transform.localScale = new Vector3(1, 1, 1);
        }

        /// <summary>
        /// Good place to update window size, and shader variables
        /// </summary>
        private void Update()
        {
            if (mCamera == null || mCamera.targetTexture == null)
            {
                return;
            }

            // Check for window resize
            if (mPreviousScreenSize.y != Screen.height ||
                mPreviousScreenSize.x != Screen.width)
            {
                mPreviousScreenSize.x = Screen.width;
                mPreviousScreenSize.y = Screen.height;

                WindowResize();
            }
        }

        private void RenderPixelSurfaces()
        {
            if (mCamera == null || mCamera.targetTexture == null || mRetroBlitAPI == null || mRetroBlitAPI.Renderer == null || !mRetroBlitAPI.Initialized)
            {
                return;
            }

            if (!mPresentEnabled)
            {
                return;
            }

            int usedBuffers = 0;
            var frontBuffers = mRetroBlitAPI.Renderer.GetFrontBuffer().GetBuffers(out usedBuffers);

            for (int bufferIndex = 0; bufferIndex < usedBuffers; bufferIndex++)
            {
                var buffer = frontBuffers[bufferIndex];
                var effectParams = buffer.effectParams;

                SetShaderGlobals(effectParams, buffer.tex);

                Vector2i displaySize = RB.DisplaySize;
                if (mRetroBlitAPI.HW.PixelStyle == RB.PixelStyle.Wide)
                {
                    displaySize.x *= 2;
                }
                else if (mRetroBlitAPI.HW.PixelStyle == RB.PixelStyle.Tall)
                {
                    displaySize.y *= 2;
                }

                mPresentSize.width = Screen.width;
                mPresentSize.height = (int)(Screen.width * ((float)displaySize.y / (float)displaySize.x));
                if (mPresentSize.height > Screen.height)
                {
                    mPresentSize.width = (int)(Screen.height * ((float)displaySize.x / (float)displaySize.y));
                    mPresentSize.height = Screen.height;
                }

                // Round up present size to the next multiple of scanline pattern length. Without this we can get bad repetition
                // patterns in the scanline effect
                // At most this will cut off a part of 1 pixel.
                int offset;
                int length;
                float pixelSize = mPresentSize.height / (float)displaySize.y;
                mRetroBlitAPI.Renderer.GetScanlineOffsetLength(pixelSize, out offset, out length);

                if (mPresentSize.width % length > 0)
                {
                    mPresentSize.width += length - (mPresentSize.width % length);
                }

                if (mPresentSize.height % length > 0)
                {
                    mPresentSize.height += length - (mPresentSize.height % length);
                }

                mPresentOffset.x = (int)((Screen.width - mPresentSize.width) / 2.0f);
                mPresentOffset.y = (int)((Screen.height - mPresentSize.height) / 2.0f);

                var clipRect = new Rect(mPresentOffset.x, mPresentOffset.y, mPresentSize.width, mPresentSize.height);

                // Slide effect
                Vector2i slideOffset = effectParams[(int)RB.Effect.Slide].Vector;
                Vector2 slideOffsetf = new Vector2(
                    ((float)slideOffset.x / (float)RB.DisplaySize.width) * mPresentSize.width,
                    ((float)slideOffset.y / (float)RB.DisplaySize.height) * mPresentSize.height);
                mPresentOffset += new Vector2i((int)slideOffsetf.x, (int)slideOffsetf.y);

                // Clear, but only on first buffer
                if (bufferIndex == 0)
                {
                    GL.Clear(true, true, new Color32(0, 0, 0, 255));
                }

                // Wipe effect
                Rect destRect = new Rect(mPresentOffset.x, mPresentOffset.y, mPresentSize.width, mPresentSize.height);
                Rect srcRect = new Rect(0, 0, 1, 1);

                Vector2i wipe = effectParams[(int)RB.Effect.Wipe].Vector;
                Vector2 wipef = new Vector2((float)wipe.x / (float)RB.DisplaySize.width, (float)wipe.y / (float)RB.DisplaySize.height);

                if (wipe.x > 0)
                {
                    destRect.x = mPresentOffset.x + (mPresentSize.width * wipef.x);
                    destRect.width = mPresentSize.width - (mPresentSize.width * wipef.x);
                    srcRect.x = wipef.x;
                    srcRect.width = 1f - wipef.x;
                }
                else if (wipe.x < 0)
                {
                    destRect.x = mPresentOffset.x;
                    destRect.width = mPresentSize.width - (mPresentSize.width * (-wipef.x));
                    srcRect.x = 0;
                    srcRect.width = 1f - (-wipef.x);
                }

                if (wipe.y > 0)
                {
                    destRect.y = mPresentOffset.y + (mPresentSize.height * wipef.y);
                    destRect.height = mPresentSize.height - (mPresentSize.height * wipef.y);
                    srcRect.y = 0;
                    srcRect.height = 1f - wipef.y;
                }
                else if (wipe.y < 0)
                {
                    destRect.y = mPresentOffset.y;
                    destRect.height = mPresentSize.height - (mPresentSize.height * (-wipef.y));
                    srcRect.y = -wipef.y;
                    srcRect.height = 1f - (-wipef.y);
                }

                // Shake
                float shake = effectParams[(int)RB.Effect.Shake].Intensity;
                if (shake > 0)
                {
                    // Don't shake every frame, shake at a set interval, and decay the shake offset
                    // between intervals
                    if (mShakeDelay <= 0)
                    {
                        var oldRandState = Random.state;
                        Random.InitState((int)RB.Ticks);

                        float maxMag = mPresentSize.width * 0.05f;
                        destRect.x += maxMag * shake * Random.Range(-1.0f, 1.0f);
                        destRect.y += maxMag * shake * Random.Range(-1.0f, 1.0f);

                        mLastShakeOffset = new Vector2i((int)destRect.x, (int)destRect.y);
                        mShakeDelay = SHAKE_INTERVAL;

                        Random.state = oldRandState;
                    }
                    else
                    {
                        destRect.x = mLastShakeOffset.x;
                        destRect.y = mLastShakeOffset.y;
                        mLastShakeOffset.x = (int)(mLastShakeOffset.x * 0.75f);
                        mLastShakeOffset.y = (int)(mLastShakeOffset.y * 0.75f);
                        mShakeDelay -= mRetroBlitAPI.HW.UpdateInterval;
                    }
                }
                else
                {
                    mLastShakeOffset = Vector2i.zero;
                    mShakeDelay = 0;
                }

                // Zoom
                float zoom = effectParams[(int)RB.Effect.Zoom].Intensity;
                if (zoom != 1)
                {
                    destRect.width *= zoom;
                    destRect.height *= zoom;
                    destRect.x += (mPresentSize.width - destRect.width) / 2f;
                    destRect.y += (mPresentSize.height - destRect.height) / 2f;
                }

                GL.PushMatrix();

                float rotation = effectParams[(int)RB.Effect.Rotation].Intensity;

                if (rotation != 0)
                {
                    GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);

                    var matrix = Matrix4x4.TRS(new Vector3(Screen.width / 2, Screen.height / 2, 0), Quaternion.identity, Vector3.one);
                    matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotation), Vector3.one);
                    matrix *= Matrix4x4.TRS(new Vector3(-Screen.width / 2, -Screen.height / 2, 0), Quaternion.identity, Vector3.one);
                    GL.MultMatrix(matrix);
                }
                else
                {
                    GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
                }

                // Do not try to render at all if currently offscreen (eg due to slide effect)
                if (!(destRect.x + destRect.width < clipRect.x ||
                    destRect.x >= clipRect.x + clipRect.width ||
                    destRect.y + destRect.height < clipRect.y ||
                    destRect.y >= clipRect.y + clipRect.height))
                {
                    if ((int)destRect.x < (int)clipRect.x)
                    {
                        var correction = clipRect.x - destRect.x;
                        srcRect.x += correction / destRect.width;
                        srcRect.width -= correction / destRect.width;
                        destRect.x = clipRect.x;
                        destRect.width -= correction;
                    }
                    else if ((int)(destRect.x + destRect.width) > (int)(clipRect.x + clipRect.width))
                    {
                        var correction = (destRect.x + destRect.width) - (clipRect.x + clipRect.width);
                        srcRect.width = (destRect.width - correction) / destRect.width;
                        destRect.width -= correction;
                    }

                    if ((int)destRect.y < (int)clipRect.y)
                    {
                        var correction = clipRect.y - destRect.y;
                        srcRect.height = (destRect.height - correction) / destRect.height;
                        destRect.y = clipRect.y;
                        destRect.height -= correction;
                    }
                    else if ((int)(destRect.y + destRect.height) > (int)(clipRect.y + clipRect.height))
                    {
                        var correction = (destRect.y + destRect.height) - (clipRect.y + clipRect.height);
                        srcRect.y += correction / destRect.height;
                        srcRect.height -= correction / destRect.height;
                        destRect.height -= correction;
                    }

                    Graphics.DrawTexture(destRect, mCamera.targetTexture, srcRect, 0, 0, 0, 0, mCurrentPresentMaterial);
                }

                GL.PopMatrix();

                buffer.tex.filterMode = FilterMode.Point;
            }
        }

        private void RenderUser()
        {
            if (mCamera != null && mCamera.targetTexture != null && mRetroBlitAPI != null && mRetroBlitAPI.Renderer != null && mRetroBlitAPI.Initialized)
            {
                mRetroBlitAPI.Renderer.RenderEnabled = true;

                mRetroBlitAPI.Renderer.StartRender();

                if (RB.Game != null)
                {
                    RB.Game.Render();
                }

                if (mRetroBlitAPI.Perf != null)
                {
                    mRetroBlitAPI.Perf.RenderEvent();
                }

                mRetroBlitAPI.Perf.Draw();

                mRetroBlitAPI.Renderer.FrameEnd();

                mRetroBlitAPI.Renderer.RenderEnabled = false;
            }
        }

#if RETROBLIT_STANDALONE
        private void OnPostRender()
        {
            RenderUser();
        }

        private void OnFinalRender()
        {
            RenderPixelSurfaces();
        }
#else
        /// <summary>
        /// First ask the user to do their rendering, then wait for end of frame, and finally display to screen
        /// </summary>
        /// <returns>Enumerator</returns>
        private IEnumerator OnPostRender()
        {
            RenderUser();

            // The yield causes some GC cost, 40 bytes in profiler. Not sure how to get around that...
            yield return mEndOfFrameWait;

            RenderPixelSurfaces();
        }
#endif
    }
}
