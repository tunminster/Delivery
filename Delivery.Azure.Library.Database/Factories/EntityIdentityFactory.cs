using System;
using System.Text;
using Delivery.Azure.Library.Sharding.Interfaces;

namespace Delivery.Azure.Library.Database.Factories
{
    /// <summary>
    ///  Generate external id
    /// </summary>
    public static class EntityIdentityFactory
    {
        public static string GenerateExternalId(this IShard shard, int idLength = 24, string? randomPortion = null,
            string? randomPortionPrefix = null)
        {
            var stringBuilder = new StringBuilder();

            var generateRandomReadableId =
                string.IsNullOrEmpty(randomPortion) ? GenerateRandomReadableId(idLength) : randomPortion;

            stringBuilder.Append(generateRandomReadableId);

            randomPortion = stringBuilder.ToString().ToLowerInvariant();
            var id = $"{shard.Key.ToLowerInvariant()}-{randomPortionPrefix}{randomPortion}";
            var trimmedId = id.Substring(startIndex: 0, idLength);
            return trimmedId;
        }

        public static string GenerateRandomReadableId(int idLength = 24)
        {
            var stringBuilder = new StringBuilder();
            var randomId = new StringBuilder().Append(Guid.NewGuid().ToString().Replace("-", string.Empty));

            while (randomId.Length < idLength)
            {
                randomId.Append(Guid.NewGuid().ToString().Replace("-", string.Empty));
            }

            for (var i = 0; i < idLength; i += 4)
            {
                var prefix = i == 0 ? string.Empty : "-";
                var portion = $"{prefix}{randomId.ToString().Substring(i, length: 4)}";
                stringBuilder.Append(portion);
            }

            return stringBuilder.ToString();

        }
    }
}