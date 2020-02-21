namespace RetroBlitInternal
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using UnityEngine;

    /// <summary>
    /// Some utility methods used throughout RetroBlit
    /// </summary>
    public static class RetroBlitUtil
    {
#if UNITY_EDITOR
        private static Dictionary<string, int> mSpamCounter = new Dictionary<string, int>();
#endif

        /// <summary>
        /// Wrap an angle to a value between 0-360 degrees
        /// </summary>
        /// <param name="rotation">Angle</param>
        /// <returns>Wrapped angle</returns>
        public static float WrapAngle(float rotation)
        {
            rotation = rotation % 360;

            if (rotation < 0)
            {
                rotation += 360;
            }

            return rotation;
        }

        /// <summary>
        /// Log an error only once, to avoid spamming
        /// </summary>
        /// <param name="str">Error string</param>
        public static void LogErrorOnce(string str)
        {
            LogOnce(1, str);
        }

        /// <summary>
        /// Log info only once, to avoid spamming
        /// </summary>
        /// <param name="str">Error string</param>
        public static void LogInfoOnce(string str)
        {
            LogOnce(0, str);
        }

        /// <summary>
        /// Low quality but fast random number generator
        /// </summary>
        /// <param name="seed">Seed, will be updated</param>
        /// <returns>32 bit random unsigned number</returns>
        public static uint RandFast(ref uint seed)
        {
            // XorShift implementation
            uint x = seed ^ 0x291D8A1; // Make sure it's never 0
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            seed = x;

            return x;
        }

        /// <summary>
        /// Fast branch-less absolute value. Incorrect results for Int.MinValue.
        /// </summary>
        /// <param name="x">Value</param>
        /// <returns>Absolute value</returns>
        public static int FastIntAbs(int x)
        {
            return (x ^ (x >> 31)) - (x >> 31);
        }

        /// <summary>
        /// A stable hash, gives the same hash value on any platform. Treats forward and backward slashes as the same.
        /// The resulting hash value will never be 0 or -1, unless string is null
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns>Hash value</returns>
        public static int StableStringHash(string str)
        {
            if (str == null)
            {
                return 0;
            }

            int hash = 5381;

            int len = str.Length;
            for (int i = 0; i < len; i++)
            {
                int c;
                if (str[i] == '\\')
                {
                    c = (int)'/';
                }
                else
                {
                    c = (int)str[i];
                }

                hash = ((hash << 5) + hash) + c;
            }

            if (hash == 0 || hash == -1)
            {
                hash += 128;
            }

            return hash;
        }

        /// <summary>
        /// A stable hash, gives the same hash value on any platform. Treats forward and backward slashes as the same.
        /// The resulting hash value will never be 0 or -1, unless string is null
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns>Hash</returns>
        public static int StableStringHash(FastString str)
        {
            if (str == null)
            {
                return 0;
            }

            int hash = 5381;

            int len = str.Length;
            for (int i = 0; i < len; i++)
            {
                int c;
                if (str[i] == '\\')
                {
                    c = (int)'/';
                }
                else
                {
                    c = (int)str[i];
                }

                hash = ((hash << 5) + hash) + c;
            }

            if (hash == 0 || hash == -1)
            {
                hash += 128;
            }

            return hash;
        }

        /// <summary>
        /// Log a string only once
        /// </summary>
        /// <param name="severity">Severity level</param>
        /// <param name="str">String</param>
        private static void LogOnce(int severity, string str)
        {
#if UNITY_EDITOR
            var sf = new System.Diagnostics.StackTrace(true).GetFrame(1);
            string id = sf.GetFileName() + ":" + sf.GetFileLineNumber();
            if (!mSpamCounter.ContainsKey(id) || mSpamCounter[id] == 0)
            {
                mSpamCounter[id] = 1;
            }
            else
            {
                // Already logged once
                return;
            }
#endif
            if (severity == 1)
            {
                Debug.LogError(str);
            }
            else
            {
                Debug.Log(str);
            }
        }
    }
}
