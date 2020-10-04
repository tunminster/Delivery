using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Delivery.Azure.Library.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static string Format(this object? target)
        {
            if (target == null)
            {
                return "<empty>";
            }

            if (target is string)
            {
                if (target.ToString() == string.Empty)
                {
                    return "empty";
                }

                return target.ToString() ?? string.Empty;
            }

            if (target is DateTime time)
            {
                return $"{time.Year}-{time.Month:00}-{time.Day:00}T:{time.Hour:00}:{time.Minute:00}:{time.Second}";
            }

            if (target is DateTimeOffset offset)
            {
#if DEBUG
                // for reasons that are not clear, NUnit always shows test cases as inconclusive when full datetime is used in a contract ToString
                return $"{offset.Year}-{offset.Month:00}-{offset.Day:00}";
#else
					return offset.ToString();
#endif
            }

            if (!(target is IEnumerable))
            {
                return target.ToString() ?? string.Empty;
            }

            switch (target)
            {
                case IDictionary dictionary:
                    {
                        var targetCopy = target;
                        target = dictionary.Keys.OfType<object>().Select(key => $"{key}: {((IDictionary)targetCopy)[key]}");
                        break;
                    }
                case IEnumerable<KeyValuePair<string, IEnumerable<string>>> pairs:
                    target = string.Join("\r\n", pairs.Select(keyValuePair => $"{keyValuePair.Key}: {string.Join("\r\n", keyValuePair.Value.Select(Format))}"));
                    break;
            }

            if (target is string)
            {
                return Convert.ToString(target) ?? string.Empty;
            }

            var enumerable = ((IEnumerable)target).Cast<object>();
            return !enumerable.Any() ? "collection empty" : string.Join(", ", enumerable.Select(Format));

            // ReSharper disable All - recheck when .net core 3.0 is GA
            // ReSharper restore All - recheck when .net core 3.0 is GA
        }
    }
}
