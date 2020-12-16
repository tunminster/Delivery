using System;
using Delivery.Azure.Library.Core.Extensions.DateTime;

namespace Delivery.Azure.Library.Core
{
    public class DaysCoveredCalculator
    {
        private readonly DateTimeOffset target;
        private readonly DateTimeOffset coverageFrom;
        private readonly DateTimeOffset coverageTo;

        public DaysCoveredCalculator(DateTimeOffset target, DateTimeOffset coverageFrom, DateTimeOffset coverageTo)
        {
            this.target = target;
            this.coverageFrom = coverageFrom;
            this.coverageTo = coverageTo;
        }

        private int DaysInMonth => target.DaysInMonth();
        private DateTimeOffset FirstDayOfMonth => target.FirstDayOfMonth();
        private DateTimeOffset LastDayOfMonth => target.LastDayOfMonth();
        private bool ExpressionA => coverageFrom <= FirstDayOfMonth && coverageTo >= LastDayOfMonth;
        private bool ExpressionB => coverageFrom > FirstDayOfMonth && coverageTo >= LastDayOfMonth;
        private bool ExpressionC => coverageFrom > FirstDayOfMonth && coverageTo < LastDayOfMonth;
        private bool ExpressionD => coverageFrom <= FirstDayOfMonth && coverageTo < LastDayOfMonth;

        public int GetDaysCovered()
        {
            if (ExpressionA)
            {
                return DaysInMonth;
            }

            if (ExpressionB)
            {
                return (coverageFrom - FirstDayOfMonth).Days;
            }

            if (ExpressionC)
            {
                return (coverageFrom - coverageTo).Days;
            }

            if (ExpressionD)
            {
                return (FirstDayOfMonth - coverageTo).Days;
            }

            return 0;
        }
    }
}