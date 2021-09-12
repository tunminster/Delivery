using System;
using System.Linq;
using System.Security;
using System.Security.Claims;
using Delivery.Azure.Library.Configuration.Environments.Interfaces;
using Delivery.Azure.Library.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry
{
    /// <summary>
	///     Extends <see cref="HttpRequest" /> and <see cref="HttpResponse" />
	/// </summary>
	public static class HttpMessageExtensions
	{
		/// <summary>
		///     Gets the correlation Id for the request from the properties.
		///     Returns empty string when nothing was found
		/// </summary>
		public static string GetCorrelationId(this HttpRequest httpRequest)
		{
			// Take "Request-Id" HTTP header as priority for cross-service tracking
			httpRequest.Headers.TryGetValue(HttpHeaders.CorrelationId, out var correlationId);
			if (!string.IsNullOrWhiteSpace(correlationId))
			{
				return correlationId;
			}

			return httpRequest.HttpContext.TraceIdentifier;
		}

		/// <summary>
		///     Gets the user email calling the api from api management
		/// </summary>
		public static string GetUserEmail(this HttpRequest httpRequest)
		{
			var httpContextUser = httpRequest.HttpContext.User;
			if (httpContextUser.Identity?.IsAuthenticated ?? false)
			{
				var identityName = httpContextUser.Identity.Name;

				var nameIdentifier = httpContextUser.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

				if (nameIdentifier != null)
				{
					return nameIdentifier.Value;
				}
				
				if (string.IsNullOrEmpty(identityName))
				{
					throw new SecurityException($"User is authenticated ({httpContextUser.Identity}), but no name is set");
				}

				return identityName;
			}

			httpRequest.Headers.TryGetValue(HttpHeaders.UserEmail, out var userEmail);
			return userEmail;
		}

		/// <summary>
		///     Gets the ring for the request from the properties or from the configuration
		/// </summary>
		public static int? GetRing(this HttpRequest httpRequest)
		{
			httpRequest.Headers.TryGetValue(HttpHeaders.Ring, out var ring);
			if (string.IsNullOrEmpty(ring))
			{
				ring = httpRequest.HttpContext.RequestServices.GetRequiredService<IEnvironmentProvider>().GetCurrentRing()?.ToString();
			}

			int.TryParse(ring, out var parsedRing);
			return parsedRing;
		}

		/// <summary>
		///     Assigns the CorrelationId property of the http context items to a given correlation id, or creates a new one if not
		///     already set
		/// </summary>
		/// <param name="httpRequest">The request which the correlation id represents</param>
		/// <param name="correlationId">
		///     Can be optionally set to a specific correlation id if the correlation id contains logic
		///     such as a timestamp or origin
		/// </param>
		/// <returns>The created or used correlation id</returns>
		public static string SetCorrelationId(this HttpRequest httpRequest, string? correlationId = null)
		{
			var usedCorrelationId = string.IsNullOrWhiteSpace(correlationId) ? httpRequest.GetCorrelationId() : correlationId;
			if (string.IsNullOrEmpty(usedCorrelationId))
			{
				usedCorrelationId = Guid.NewGuid().ToString();
			}

			httpRequest.HttpContext.TraceIdentifier = usedCorrelationId;

			return usedCorrelationId;
		}

		/// <summary>
		///  Set user email header
		/// </summary>
		/// <param name="httpRequest"></param>
		/// <returns></returns>
		public static string SetUserEmail(this HttpRequest httpRequest)
		{
			var userEmail = httpRequest.GetUserEmail();
			httpRequest.Headers[HttpHeaders.UserEmail] = userEmail;
			return userEmail;
		}
	}
}