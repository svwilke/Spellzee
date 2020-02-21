namespace RetroBlitDemoStressTest
{
    using UnityEngine;

    /// <summary>
    /// Demonstrate drawing apis
    /// </summary>
    public class ScenePrimitiveStress : SceneStress
    {
        /// <summary>
        /// Run the stress test
        /// </summary>
        protected override void StressTest()
        {
            Random.InitState(0);

            for (int i = 0; i < mStressLevel; i++)
            {
                var wiggle = GetWiggle();

                var randRect = new Rect2i(
                    (int)(Random.Range(-RB.SpriteSize(0).width * 0.75f, RB.DisplaySize.width - (RB.SpriteSize(0).width * 0.25f)) + wiggle.x),
                    (int)(Random.Range(-RB.SpriteSize(0).height * 0.75f, RB.DisplaySize.height - (RB.SpriteSize(0).height * 0.25f)) + wiggle.y),
                    Random.Range(8, 64),
                    Random.Range(8, 64));

                int type = Random.Range(0, 5);
                if (type == 0)
                {
                    RB.DrawLine(new Vector2i(randRect.x, randRect.y), new Vector2i(randRect.x + randRect.width, randRect.y + randRect.height), mRandomColor[i]);
                }
                else if (type == 1)
                {
                    RB.DrawRect(randRect, mRandomColor[i]);
                }
                else if (type == 2)
                {
                    RB.DrawRectFill(randRect, mRandomColor[i]);
                }
                else if (type == 3)
                {
                    RB.DrawTriangleFill(
                        new Vector2i(randRect.x, randRect.y),
                        new Vector2i(randRect.x + randRect.width, randRect.y),
                        new Vector2i(randRect.x + (randRect.width / 2), randRect.y + randRect.height),
                        mRandomColor[i]);
                }
                else if (type == 4)
                {
                    RB.DrawTriangle(
                        new Vector2i(randRect.x, randRect.y),
                        new Vector2i(randRect.x + randRect.width, randRect.y),
                        new Vector2i(randRect.x + (randRect.width / 2), randRect.y + randRect.height),
                        mRandomColor[i]);
                }
            }
        }

        /// <summary>
        /// Draw information overlay
        /// </summary>
        protected override void Overlay()
        {
            mString.Clear();
            mString.Append("Primitive Stress Test\n@7F7F7FPrimitives: @FFFFFF").Append(mStressLevel).Append("\n@7F7F7FFPS: @FFFFFF").Append(mFPS, 2).Append("\n\n");
            mString.Append("\u2191 \u2193 @7F7F7FChange Stress Level\n").Append("@FFFFFF");
            mString.Append("\u2190 \u2192 @7F7F7FChange Test");

            RB.DrawRectFill(new Rect2i(4, 4, 115, 51), new Color32(32, 32, 32, 255));
            RB.Print(new Vector2i(6, 6), Color.white, mString);
        }
    }
}
