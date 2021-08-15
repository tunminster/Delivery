using System;
using System.Collections.Concurrent;

namespace Delivery.Azure.Library.WebApi.Swagger.Parameterization
{
    public class OpenApiExampleParameters : ConcurrentDictionary<string, Func<object, int, string>>
	{
		private static readonly DateTimeOffset currentFixedDateTimeUtc = DateTimeOffset.UtcNow;
		private const string LiveDateTimeUtcKey = "CurrentLiveDateTimeUtc:date";

		public OpenApiExampleParameters()
		{
			// ReSharper disable All
			// dates
			TryAdd("CurrentFixedDateTimeUtc:date", (_, _) => currentFixedDateTimeUtc.ToString());
			TryAdd("CurrentFixedDateTimeUtc:date-only", (_, _) => currentFixedDateTimeUtc.Date.ToString());
			TryAdd("CurrentFixedDateTimePlusOneDayUtc:date", (_, _) => currentFixedDateTimeUtc.AddDays(days: 1).ToString());
			TryAdd("CurrentFixedDateTimePlusOneMonthUtc:date", (_, _) => currentFixedDateTimeUtc.AddMonths(months: 1).ToString());
			TryAdd("CurrentFixedDateTimePlusTwoMonthsUtc:date", (_, _) => currentFixedDateTimeUtc.AddMonths(months: 2).ToString());
			TryAdd("CurrentFixedDateTimePlusOneYearUtc:date", (_, _) => currentFixedDateTimeUtc.AddYears(years: 1).ToString());
			TryAdd("CurrentFixedDateTimePlusTwoYearsUtc:date", (_, _) => currentFixedDateTimeUtc.AddYears(years: 2).ToString());
			TryAdd("CurrentFixedDateTimeMinusOneMonthUtc:date", (_, _) => currentFixedDateTimeUtc.AddMonths(months: -1).ToString());
			TryAdd("CurrentFixedDateTimeMinusTwoMonthsUtc:date", (_, _) => currentFixedDateTimeUtc.AddMonths(months: -2).ToString());
			TryAdd("CurrentFixedDateTimeMinusOneYearUtc:date", (_, _) => currentFixedDateTimeUtc.AddYears(years: -1).ToString());
			TryAdd("CurrentFixedDateTimeMinusTwoYearsUtc:date", (_, _) => currentFixedDateTimeUtc.AddYears(years: -2).ToString());
			TryAdd("CurrentFixedDateTimeMinusOneDayUtc:date", (_, _) => currentFixedDateTimeUtc.AddDays(days: -1).ToString());
			TryAdd("CurrentFixedDateTimeMinusTwoDaysUtc:date", (_, _) => currentFixedDateTimeUtc.AddDays(days: -2).ToString());

			// allow tests which depend on incrementing dates to achieve this
			TryAdd(LiveDateTimeUtcKey, (_, _) => DateTimeOffset.UtcNow.ToString());
			// ReSharper restore All
		}
	}
}