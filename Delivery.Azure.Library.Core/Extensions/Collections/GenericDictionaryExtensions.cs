using System;
using System.Collections.Generic;

namespace Delivery.Azure.Library.Core.Extensions.Collections
{
    /// <summary>
    ///     Extends the dictionary to allow easy adding and updating of items
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class GenericDictionaryExtensions
    {
        /// <summary>
        ///     Merges two dictionaries into one.
        ///     If a key already exists in the destination dictionary it will not be added nor updated.
        /// </summary>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <param name="destination">Dictionary that will contain all the items</param>
        /// <param name="source">Dictionary to copy from</param>
        /// <returns>Merged dictionary</returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> destination, IDictionary<TKey, TValue> source)
            where TKey : class
        {
            return Merge(destination, source, manipulationFunc: null);
        }

        /// <summary>
        ///     Merges two dictionaries into one.
        ///     If a key already exists in the destination dictionary it will not be added nor updated.
        /// </summary>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <param name="destination">Dictionary that will contain all the items</param>
        /// <param name="source">Dictionary to copy from</param>
        /// <param name="manipulationFunc">Function to manipulate the data before copying</param>
        /// <returns>Merged dictionary</returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> destination, IDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, KeyValuePair<TKey, TValue>>? manipulationFunc)
            where TKey : class
        {
            source.ForEach(pair =>
            {
                if (!destination.ContainsKey(pair.Key))
                {
                    if (manipulationFunc != null)
                    {
                        pair = manipulationFunc(pair);
                    }

                    destination.Add(pair.Key, pair.Value);
                }
            });

            return destination;
        }
    }
}