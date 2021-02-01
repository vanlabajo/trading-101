using System;
using System.Collections.Generic;

namespace TradingStrategies.Constants
{
    static class UsTradingHours
    {
        /// <summary>
        /// 09:30 AM Eastern Time
        /// </summary>
        public static readonly TimeSpan Start;

        /// <summary>
        /// 04:00 PM Eastern Time
        /// </summary>
        public static readonly TimeSpan End;

        /// <summary>
        /// Monday to Friday
        /// </summary>
        public static readonly List<DayOfWeek> Weekdays;

        static UsTradingHours()
        {
            Start = new TimeSpan(9, 30, 0);
            End = new TimeSpan(16, 0, 0);
            Weekdays = new List<DayOfWeek>
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            };
        }
    }
}
