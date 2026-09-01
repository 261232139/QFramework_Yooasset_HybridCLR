using System;
using UnityEngine;

namespace HotUpdate.Utils
{
    /// <summary>
    /// Provides game time, server-time calibration, Unix timestamp conversion, and countdown formatting.
    /// </summary>
    public static class TimeUtil
    {
        public const int SecondsOneDay = 24 * 60 * 60;
        public const int DaysInWeek = 7;
        public const int DAYS_IN_WEEK = DaysInWeek;

        private static DateTimeOffset? s_NetworkUtcTime;
        private static double s_NetworkTimeRealtime;

        public static bool HasNetworkTime => s_NetworkUtcTime.HasValue;

        public static bool IsUsingNetworkTime()
        {
            return HasNetworkTime;
        }

        public static void SetTimeScale(float scale)
        {
            if (scale < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), "Time scale cannot be negative.");
            }

            Time.timeScale = scale;
        }

        public static float GetTimeScale()
        {
            return Time.timeScale;
        }

        /// <summary>
        /// Calibrates the game clock to an authoritative server time.
        /// </summary>
        public static void RefreshNetworkTime(DateTime networkTime)
        {
            RefreshNetworkTime(new DateTimeOffset(networkTime));
        }

        /// <summary>
        /// Calibrates the game clock to an authoritative server time.
        /// </summary>
        public static void RefreshNetworkTime(DateTimeOffset networkTime)
        {
            s_NetworkUtcTime = networkTime.ToUniversalTime();
            s_NetworkTimeRealtime = Time.realtimeSinceStartupAsDouble;
        }

        public static void ClearNetworkTime()
        {
            s_NetworkUtcTime = null;
            s_NetworkTimeRealtime = 0d;
        }

        /// <summary>
        /// Gets UTC time, using the calibrated server clock when available.
        /// </summary>
        public static DateTime UtcNow()
        {
            return UtcNowOffset().UtcDateTime;
        }

        /// <summary>
        /// Gets local device time, using the calibrated server clock when available.
        /// </summary>
        public static DateTime Now()
        {
            return UtcNowOffset().ToLocalTime().DateTime;
        }

        public static DateTime GetLocalTimeFromUtc(DateTime utcTime)
        {
            if (utcTime.Kind == DateTimeKind.Unspecified)
            {
                utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime.ToUniversalTime(), TimeZoneInfo.Local);
        }

        /// <summary>
        /// Gets the current Unix timestamp in seconds. Retained for compatibility with existing callers.
        /// </summary>
        public static long GetUnixTimeStamps()
        {
            return TotalSeconds();
        }

        public static long TotalSeconds()
        {
            return UtcNowOffset().ToUnixTimeSeconds();
        }

        public static long LocalTotalSeconds()
        {
            return TotalSeconds();
        }

        public static long TotalMilliseconds()
        {
            return UtcNowOffset().ToUnixTimeMilliseconds();
        }

        public static long DateTimeToSeconds(DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }

        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }

        public static DateTime ParseTimestampToDate(long seconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        }

        public static DateTime ParseTimeMilliSecondToDate(long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
        }

        public static DateTime ParseTimeFromNow(long countdownSeconds)
        {
            return UtcNow().AddSeconds(countdownSeconds);
        }

        public static double Countdown(DateTime targetUtcTime)
        {
            return (targetUtcTime.ToUniversalTime() - UtcNow()).TotalSeconds;
        }

        public static double Countdown(long targetTimestampSeconds)
        {
            return targetTimestampSeconds - TotalSeconds();
        }

        public static double PastTime(DateTime pastUtcTime)
        {
            return (UtcNow() - pastUtcTime.ToUniversalTime()).TotalSeconds;
        }

        public static bool IsSameDayByLocalTime(long timestamp1, long timestamp2)
        {
            var first = DateTimeOffset.FromUnixTimeSeconds(timestamp1).ToLocalTime().Date;
            var second = DateTimeOffset.FromUnixTimeSeconds(timestamp2).ToLocalTime().Date;
            return first == second;
        }

        /// <summary>
        /// Gets the Unix timestamp for the next local midnight.
        /// </summary>
        public static long GetTomorrowTimestamp()
        {
            var nextMidnight = Now().Date.AddDays(1);
            return new DateTimeOffset(nextMidnight).ToUnixTimeSeconds();
        }

        public static long GetUtcZeroTimestamp()
        {
            return new DateTimeOffset(GetUtcZeroDateTime()).ToUnixTimeSeconds();
        }

        public static DateTime GetUtcZeroDateTime()
        {
            return UtcNow().Date;
        }

        /// <summary>
        /// Returns whether a local 22:00 daily reset has occurred since the supplied Unix timestamp.
        /// </summary>
        public static bool IsNewDayAfterUtc22(long lastUtcTime)
        {
            var lastReset = GetMostRecentLocalReset(DateTimeOffset.FromUnixTimeSeconds(lastUtcTime).ToLocalTime().DateTime, 22);
            var currentReset = GetMostRecentLocalReset(Now(), 22);
            return currentReset > lastReset;
        }

        public static string GetUtcDateString()
        {
            return UtcNow().ToString("O");
        }

        public static string GetUtcTimeStringISO8601()
        {
            return UtcNow().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        public static string GetTimeString(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 0d)
            {
                return "00:00";
            }

            var totalHours = (int)timeSpan.TotalHours;
            return totalHours >= 1
                ? $"{totalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
                : $"{(int)timeSpan.TotalMinutes:D2}:{timeSpan.Seconds:D2}";
        }

        public static string GetTimeCountDownStringWithDay(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays > 1d)
            {
                return timeSpan.Hours == 0 ? $"{timeSpan.Days}d" : $"{timeSpan.Days}d {timeSpan.Hours}h";
            }

            return GetTimeString(timeSpan);
        }

        /// <summary>
        /// Replaces %d, %dd, %h, %hh, %m, %mm, %s, %ss, and %ms tokens.
        /// </summary>
        public static string GetTimeString(string format, int seconds)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));

            var duration = TimeSpan.FromSeconds(Math.Max(0, seconds));
            var totalHours = (int)duration.TotalHours;
            var totalMinutes = (int)duration.TotalMinutes;
            var totalMilliseconds = (long)duration.TotalMilliseconds;

            return format
                .Replace("%ms", totalMilliseconds.ToString())
                .Replace("%dd", duration.Days.ToString("D2"))
                .Replace("%d", duration.Days.ToString())
                .Replace("%hh", (format.Contains("%d") ? duration.Hours : totalHours).ToString("D2"))
                .Replace("%h", (format.Contains("%d") ? duration.Hours : totalHours).ToString())
                .Replace("%mm", (format.Contains("%h") ? duration.Minutes : totalMinutes).ToString("D2"))
                .Replace("%m", (format.Contains("%h") ? duration.Minutes : totalMinutes).ToString())
                .Replace("%ss", duration.Seconds.ToString("D2"))
                .Replace("%s", duration.Seconds.ToString());
        }

        public static string SecondToTimeFormat24(int seconds)
        {
            if (seconds <= 0) return "00:00:00";
            return GetTimeString("%hh:%mm:%ss", seconds);
        }

        public static string SecondToTimeFormat(int seconds)
        {
            if (seconds <= 0) return "00:00";
            return seconds >= 3600 ? GetTimeString("%hh:%mm", seconds) : GetTimeString("%mm:%ss", seconds);
        }

        public static string FillFormat(int value)
        {
            return value.ToString("D2");
        }

        public static int DiffDaysByDistance(DateTime toDateTime, DateTime fromDateTime)
        {
            return (toDateTime.Date - fromDateTime.Date).Days;
        }

        public static int DiffDays(DateTime toDateTime, DateTime fromDateTime)
        {
            var dayDistance = DiffDaysByDistance(toDateTime, fromDateTime);
            return dayDistance >= 1 || toDateTime.Date != fromDateTime.Date ? Math.Max(dayDistance, 1) : 0;
        }

        public static int GetDayID(DateTime dateTime)
        {
            return dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
        }

        public static DateTime SecToDateTime(int seconds)
        {
            return ParseTimestampToDate(seconds);
        }

        public static string GetDateTimeStringForGTA(int timestampSeconds)
        {
            return SecToDateTime(timestampSeconds).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetLocalDateTimeStringForGTA(int timestampSeconds)
        {
            return SecToDateTime(timestampSeconds).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }

        private static DateTimeOffset UtcNowOffset()
        {
            if (!s_NetworkUtcTime.HasValue)
            {
                return DateTimeOffset.UtcNow;
            }

            var elapsedSeconds = Time.realtimeSinceStartupAsDouble - s_NetworkTimeRealtime;
            return s_NetworkUtcTime.Value.AddSeconds(Math.Max(0d, elapsedSeconds));
        }

        private static DateTime GetMostRecentLocalReset(DateTime localTime, int hour)
        {
            var reset = localTime.Date.AddHours(hour);
            return localTime < reset ? reset.AddDays(-1) : reset;
        }
    }
}
