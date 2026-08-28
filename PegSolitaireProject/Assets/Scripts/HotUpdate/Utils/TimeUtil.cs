using System;
using UnityEngine;

namespace HotUpdate.Utils
{
    public static class TimeUtil
    {
        public static void SetTimeScale(float scale)
        {
            if (scale < 0f) throw new ArgumentOutOfRangeException(nameof(scale), "Time scale cannot be negative.");
            Time.timeScale = scale;
        }

        public static float GetTimeScale()
        {
            return Time.timeScale;
        }

        public static long GetUnixTimeStamps()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }
    }
}
