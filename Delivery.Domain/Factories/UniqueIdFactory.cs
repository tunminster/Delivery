using System;

namespace Delivery.Domain.Factories
{
    public class UniqueIdFactory
    {
        public static string UniqueFacebookId()
        {
            var randomTenPlaces = new Random().Next(minValue: 100000000, maxValue: 999999999);
            return $"{randomTenPlaces}";
        }
    }
}