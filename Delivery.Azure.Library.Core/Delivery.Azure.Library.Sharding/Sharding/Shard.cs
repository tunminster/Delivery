using System;
using Delivery.Azure.Library.Sharding.Interfaces;

namespace Delivery.Azure.Library.Sharding.Sharding
{
    public class Shard : IShard
    {
        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="key"></param>
        private Shard(string key)
        {
            this.key = key;
        }

        /// <summary>
        /// Key of the shard
        /// </summary>
        private readonly string key;

        public string Key => key;

        /// <summary>
        ///     Creates information about a shard
        /// </summary>
        /// <param name="shardKey">Key to shard on</param>
        public static IShard Create(string shardKey)
        {
            if (string.IsNullOrEmpty(shardKey))
            {
                throw new ArgumentNullException($"{nameof(shardKey)} cannot be empty");
            }

            shardKey = shardKey.Replace(" ", string.Empty);

            return new Shard(shardKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 13;

                if (!string.IsNullOrWhiteSpace(Key))
                {
                    hashCode = hashCode * 7 * Key.ToLowerInvariant().GetHashCode();
                }

                return hashCode;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Shard other && string.Equals(other.Key, Key, StringComparison.CurrentCultureIgnoreCase);
        }

        public override string ToString()
        {
            return $"{typeof(Shard)}: {nameof(Key)} - {Key}";
        }
    }
}
