using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Core.Extensions.Http;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Core.Monads;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Exceptions;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json.Linq;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry
{
    public static class HttpTelemetryWriter
	{
		public const int MemoryBufferLoggingLimitBytes = 262144;
		public const string ModelBindingContentKey = "ModelBindingContent";

		public static async Task<HttpResponseMessage> EnsureSuccessStatusCodeExtendedAsync(this HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
			{
				throw new HttpRequestException($"Error calling service: {await response.FormatHttpResponseMessageAsync()}");
			}

			return response;
		}

		public static bool IsSuccessStatusCode(this HttpResponse response)
		{
			return response.StatusCode >= 200 && response.StatusCode <= 299;
		}

		/// <summary>
		///     Gets the raw content of a response
		/// </summary>
		/// <param name="response">Response message</param>
		/// <returns>Raw content</returns>
		public static async Task<string> ToRawContentAsync(this HttpResponse response)
		{
			if (!(response.Body is WrappedResponseBodyStream wrappedResponseBodyStream))
			{
				throw new InvalidOperationException($"Expected the response body to be converted to a {nameof(WrappedResponseBodyStream)}, however it is {response.Body.GetType().Name}");
			}

			var requestContent = await ReadBodyAsync(response.HttpContext.Request, wrappedResponseBodyStream.CopiedStream);
			return requestContent.IsPresent ? requestContent.Value : string.Empty;
		}

		/// <summary>
		///     Gives a list of all headers in a request
		/// </summary>
		/// <param name="request">Request message</param>
		/// <returns>List of all headers</returns>
		public static string ToRawHeaders(this HttpRequest request)
		{
			var stringBuilder = new StringBuilder();
			ExtractRawRequestHeaders(request, stringBuilder);

			request.Headers.ForEach(headerPair =>
			{
				if (!HttpHeadersExtensions.SensitiveHeaders.Select(sensitiveHeader => sensitiveHeader.ToLowerInvariant()).Contains(headerPair.Key.ToLowerInvariant()))
				{
					stringBuilder.AppendLine($"{headerPair.Key}: {headerPair.Value.Format()}");
				}
			});

			return stringBuilder.ToString();
		}

		private static async Task<Maybe<string>> ReadBodyAsync(HttpRequest request, Stream body)
		{
			if (request.Headers.TryGetValue("Accept", out var value) && SkipLogBody(value.ToString(), body))
			{
				// body is too large to log conventionally; should be streamed to file storage
				return Maybe<string>.NotPresent;
			}

			if (!body.CanRead || body.Length == 0)
			{
				return new Maybe<string>(string.Empty);
			}

			using var streamReader = new StreamReader(body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
			if (body.CanSeek)
			{
				body.Seek(offset: 0, SeekOrigin.Begin);
			}

			var content = await streamReader.ReadToEndAsync();
			body.Position = 0;
			return new Maybe<string>(content);
		}

		private static void ExtractRawRequestHeaders(HttpRequest request, StringBuilder stringBuilder)
		{
			stringBuilder.AppendLine($"{CustomProperties.Uri}: {request.GetEncodedUrl().Format()}");
			stringBuilder.AppendLine($"{CustomProperties.Method}: {request.Method.Format()}");
			stringBuilder.AppendLine($"{CustomProperties.ContentLength}: {request.ContentLength}");
			stringBuilder.AppendLine($"{CustomProperties.ContentType}: {request.ContentType.Format()}");
		}

		/// <summary>
		///     Reads the http request body using the pipe reader if the request is less than
		///     <see cref="MemoryBufferLoggingLimitBytes" />
		/// </summary>
		public static async Task<Maybe<string>> ReadBodyAsync(this HttpRequest httpRequest, CancellationToken cancellationToken = default)
		{
			if (httpRequest.HttpContext.Items.ContainsKey(ModelBindingContentKey))
			{
				return new Maybe<string>(httpRequest.HttpContext.Items[ModelBindingContentKey]?.ToString() ?? string.Empty);
			}

			if (SkipLogBody(httpRequest.ContentType, httpRequest.Body))
			{
				return Maybe<string>.NotPresent;
			}

			if (!httpRequest.Body.CanRead)
			{
				return Maybe<string>.NotPresent;
			}

			if (httpRequest.Body.Length == 0)
			{
				return new Maybe<string>(string.Empty);
			}

			var bodyReader = httpRequest.BodyReader;
			httpRequest.Body.Position = 0;
			var requestBodyInBytes = await bodyReader.ReadAsync(cancellationToken);
			bodyReader.AdvanceTo(requestBodyInBytes.Buffer.Start, requestBodyInBytes.Buffer.End);
			var body = Encoding.UTF8.GetString(requestBodyInBytes.Buffer.FirstSpan);
			httpRequest.Body.Position = 0;
			
			body = ReplaceSensitiveInfo(body);
			return new Maybe<string>(body);
		}

		private static bool SkipLogBody(string payloadType, Stream payload)
		{
			var contentTypeIsDocumentUpload = !string.IsNullOrEmpty(payloadType) && payloadType.ToLowerInvariant().Contains("form-data");
			return contentTypeIsDocumentUpload || payload.Length > MemoryBufferLoggingLimitBytes;
		}

		private static string ReplaceSensitiveInfo(string body)
		{
			try
			{
				var propsToMask = new HashSet<string>(new[] {"password"});

				var jsonObject = JObject.Parse(body);

				if (!jsonObject.Descendants().OfType<JProperty>().Any(x => propsToMask.Contains(x.Name)))
				{
					return body;
				}

				foreach (var p in jsonObject.Descendants().OfType<JProperty>().Where(x => propsToMask.Contains(x.Name)))
				{
					p.Value = "xxxxxx";
				}

				return jsonObject.ToString();
			}
			catch (Exception)
			{
				return body;
			}
		}
	}
}