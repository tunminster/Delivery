using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Delivery.Azure.Library.Core;
using Delivery.Azure.Library.Sharding.Exceptions;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Sharding.Sharding;
using Microsoft.AspNetCore.Http;

namespace Delivery.Azure.Library.Sharding.Extensions
{
    public static class HttpShardExtensions
	{
		/// <summary>
		///     Retrieves the shard identifier from the 'X-Shard' HTTP header
		/// </summary>
		/// <exception cref="ArgumentNullException">When no HTTP request was passed</exception>
		/// <exception cref="ShardNotFoundException">When no shard was found in the HTTP headers</exception>
		/// <exception cref="MultipleShardsFoundException">When multiple shards were found in the HTTP headers</exception>
		public static IShard GetShard(this HttpRequest httpRequest)
		{
			var shards = GetShardsForHttpRequestBase(httpRequest);
			if (shards.Count() > 1)
			{
				throw new MultipleShardsFoundException(shards);
			}

			if (!shards.Any())
			{
				throw new ShardNotFoundException();
			}

			return shards.Single();
		}

		/// <summary>
		///     Retrieves the shard identifier from the 'X-Shard' HTTP header
		/// </summary>
		/// <exception cref="ArgumentNullException">When no HTTP request was passed</exception>
		/// <exception cref="ShardNotFoundException">When no shard was found in the HTTP headers</exception>
		/// <exception cref="MultipleShardsFoundException">When multiple shards were found in the HTTP headers</exception>
		public static IShard GetShard(this HttpRequestMessage httpRequestMessage)
		{
			var tenantIdentifiers = GetShardsFromHttpHeader(httpRequestMessage);
			if (tenantIdentifiers.Count() > 1)
			{
				throw new MultipleShardsFoundException(tenantIdentifiers);
			}

			if (!tenantIdentifiers.Any())
			{
				throw new ShardNotFoundException();
			}

			return tenantIdentifiers.Single();
		}

		/// <summary>
		///     Retrieves the shard identifiers from the 'X-Shard' HTTP header
		/// </summary>
		/// <exception cref="ArgumentNullException">When no HTTP request was passed</exception>
		/// <exception cref="ShardNotFoundException">When no shard was found in the HTTP headers</exception>
		public static IEnumerable<IShard> GetShards(this HttpRequestMessage httpRequestMessage)
		{
			var tenantIdentifiers = GetShardsFromHttpHeader(httpRequestMessage);
			if (!tenantIdentifiers.Any())
			{
				throw new ShardNotFoundException();
			}

			return tenantIdentifiers;
		}

		/// <summary>
		///     Retrieves all the shard identifiers from the 'X-Shard' HTTP header
		/// </summary>
		/// <exception cref="ArgumentNullException">When no HTTP request was passed</exception>
		/// <exception cref="ShardNotFoundException">When no shard was found in the HTTP headers</exception>
		public static IEnumerable<IShard> GetShards(this HttpRequest httpRequest)
		{
			var shards = GetShardsForHttpRequestBase(httpRequest);
			if (!shards.Any())
			{
				throw new ShardNotFoundException();
			}

			return shards;
		}

		private static IEnumerable<IShard> ExtractShards(string rawShards)
		{
			if (string.IsNullOrWhiteSpace(rawShards))
			{
				return Array.Empty<IShard>();
			}

			var shards = rawShards.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);

			return shards.Select(Shard.Create);
		}

		private static string GetRawShardHeaderForHttpRequestBase(HttpRequest httpRequest)
		{
			if (httpRequest.Headers.TryGetValue(HttpHeaders.Shard, out var rawHeaderValue))
			{
				return rawHeaderValue;
			}

			throw new ShardNotFoundException();
		}

		private static IEnumerable<IShard> GetShardsForHttpRequestBase(HttpRequest httpRequest)
		{
			var rawHeaderValue = GetRawShardHeaderForHttpRequestBase(httpRequest);

			var shards = ExtractShards(rawHeaderValue);

			return shards;
		}

		private static IEnumerable<IShard> GetShardsFromHttpHeader(HttpRequestMessage httpRequestMessage)
		{
			if (httpRequestMessage.Headers.TryGetValues(HttpHeaders.Shard, out var tenantHeaders))
			{
				var shards = new List<IShard>();

				foreach (var tenantHeader in tenantHeaders)
				{
					var extractedShards = ExtractShards(tenantHeader);
					shards.AddRange(extractedShards);
				}

				return shards;
			}

			throw new ShardNotFoundException();
		}
	}
}