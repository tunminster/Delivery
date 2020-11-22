using System;

namespace Delivery.Azure.Library.Core.Extensions.DateTime
{
    /// <summary>
    ///     Extends DateTimeOffset with some conveniences
    /// </summary>
    public static class DateTimeOffsetExtensions
    {
        /// <summary>
        ///     Returns first day of previous month
        /// </summary>
        public static DateTimeOffset FirstDayOfPreviousMonth(this DateTimeOffset target)
        {
            return target.AddMonths(months: -1).AddDays(-(System.DateTime.Today.Day - 1));
        }

        /// <summary>
        ///     Returns first day of current month
        /// </summary>
        public static DateTimeOffset FirstDayOfMonth(this DateTimeOffset target)
        {
            return new DateTimeOffset(target.Year, target.Month, day: 1, hour: 0, minute: 0, second: 0, TimeSpan.Zero);
        }

        /// <summary>
        ///     Returns last day of month
        /// </summary>
        public static DateTimeOffset LastDayOfMonth(this DateTimeOffset target)
        {
            return new DateTimeOffset(target.Year, target.Month, target.DaysInMonth(), hour: 0, minute: 0, second: 0, TimeSpan.Zero);
        }

        /// <summary>
        ///     Returns last day of previous month
        /// </summary>
        public static DateTimeOffset LastDayOfPreviousMonth(this DateTimeOffset target)
        {
            return target.Date.AddDays(-target.Day);
        }

        /// <summary>
        ///     Returns days in a month
        /// </summary>
        public static int DaysInMonth(this DateTimeOffset target)
        {
            return System.DateTime.DaysInMonth(target.Year, target.Month);
        }

        /// <summary>
        ///     Returns no of days covered within a target month
        /// </summary>
        public static int DaysCovered(this DateTimeOffset target, DateTimeOffset coverageFrom, DateTimeOffset coverageTo)
        {
            return new DaysCoveredCalculator(target, coverageFrom, coverageTo).GetDaysCovered();
        }
    }
}