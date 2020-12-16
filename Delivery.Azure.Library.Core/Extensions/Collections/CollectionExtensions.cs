using System;
using System.Collections.Generic;

namespace Delivery.Azure.Library.Core.Extensions.Collections
{
    /// <summary>
    ///     Provides extensions applicable to all enumerables or collections
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Loops through each items in a functional way
        /// </summary>
        /// <remarks>For performance reasons do not use this for very large lists</remarks>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (var item in enumeration)
            {
                action(item);
            }
        }

        /// <summary>
        ///     Adds a range of items to the destination collection. Skips items which already exist in the source
        /// </summary>
        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                if (!destination.Contains(item))
                {
                    destination.Add(item);
                }
            }
        }
    }
}