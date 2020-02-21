namespace RetroBlitDemoStressTest
{
    using UnityEngine;

    /// <summary>
    /// Demonstrate drawing apis
    /// </summary>
    public class ScenePixelStress : SceneStress
    {
        /// <summary>
        /// Run the stress test
        /// </summary>
        protected override void StressTest()
        {
            for (int i = 0; i < mStressLevel; i++)
            {
                var randPos = mRandomPos[i];
                randPos += GetWiggle();

                RB.DrawPixel(randPos, mRandomColor[i]);
            }
        }

        /// <summary>
        /// Draw information overlay
        /// </summary>
        protected override void Overlay()
        {
            mString.Clear();
            mString.Append("Pixel Stress Test\n@7F7F7FPixels: @FFFFFF").Append(mStressLevel).Append("\n@7F7F7FFPS: @FFFFFF").Append(mFPS, 2).Append("\n\n@FFFFFF");
            mString.Append("\u2191 \u2193 @7F7F7FChange Stress Level\n").Append("@FFFFFF");
            mString.Append("\u2190 \u2192 @7F7F7FChange Test");

            RB.DrawRectFill(new Rect2i(4, 4, 115, 51), new Color32(32, 32, 32, 255));
            RB.Print(new Vector2i(6, 6), Color.white, mString);
        }
    }
}
