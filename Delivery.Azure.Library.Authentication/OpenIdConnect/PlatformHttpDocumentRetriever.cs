using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Exceptions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;

namespace Delivery.Azure.Library.Authentication.OpenIdConnect
{
    public class PlatformHttpDocumentRetriever : IDocumentRetriever
	{
		private readonly IServiceProvider serviceProvider;
		private static readonly ConcurrentDictionary<string, string> cachedDocuments = new ConcurrentDictionary<string, string>();

		public PlatformHttpDocumentRetriever(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public async Task<string> GetDocumentAsync(string address, CancellationToken cancel)
		{
			if (cachedDocuments.TryGetValue(address, out var cachedValue))
			{
				return cachedValue;
			}

			using var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
			using var documentResponseMessage = await httpClient.GetAsync(address, HttpCompletionOption.ResponseHeadersRead, cancel);
			if (!documentResponseMessage.IsSuccessStatusCode)
			{
				var message = $"Failed to download document metadata from {address}: {await documentResponseMessage.FormatHttpResponseMessageAsync()}";
				serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace(message, SeverityLevel.Error);
				throw new InvalidOperationException(message);
			}

			var contentStream = await documentResponseMessage.Content.ReadAsStreamAsync(cancel);
			using var streamReader = new StreamReader(contentStream);
			var content = await streamReader.ReadToEndAsync();
			cachedDocuments.TryAdd(address, content);
			return content;
		}
	}
}