using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using Delivery.Azure.Library.Core;
using Delivery.Azure.Library.Core.Exceptions;
using Delivery.Azure.Library.Exceptions.Writers;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Validation;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using HttpHeaders = Delivery.Azure.Library.Core.HttpHeaders;

namespace Delivery.Azure.Library.WebApi.Middleware
{
    /// <summary>
	///     Based on the <see cref="ExceptionHandlerMiddleware" /> from the .net core framework
	/// </summary>
	public class ExceptionHandlingMiddleware
	{
		private readonly Func<object, Task> clearCacheHeadersDelegate;
		private readonly DiagnosticSource diagnosticSource;
		private readonly ILogger logger;
		private readonly RequestDelegate next;
		private readonly ExceptionHandlerOptions options;
		private readonly IServiceProvider serviceProvider;

		public ExceptionHandlingMiddleware(IServiceProvider serviceProvider, RequestDelegate next,
			ILoggerFactory loggerFactory, IOptions<ExceptionHandlerOptions> options, DiagnosticSource diagnosticSource)
		{
			this.serviceProvider = serviceProvider;
			this.next = next;
			this.options = options.Value;
			this.diagnosticSource = diagnosticSource;

			logger = loggerFactory.CreateLogger<ExceptionHandlerMiddleware>();
			if (this.options.ExceptionHandler == null)
			{
				this.options.ExceptionHandler = this.next;
			}

			clearCacheHeadersDelegate = ClearCacheHeadersAsync;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await next(context);
			}
			catch (ValidationException validationException)
			{
				await HandleValidationExceptionAsync(context, validationException);
			}
			catch (SecurityTokenInvalidIssuerException securityTokenInvalidIssuerException)
			{
				await HandleUnauthorizedExceptionAsync(context, securityTokenInvalidIssuerException);
			}
			catch (XmlException xmlException)
			{
				await HandleValidationExceptionAsync(context, new ValidationException(xmlException.Message, xmlException));
			}
			catch (Exception exception)
			{
				await HandleExceptionAsync(context, exception);
			}
		}

		private static async Task ClearCacheHeadersAsync(object state)
		{
			var response = (HttpResponse) state;
			response.Headers[HeaderNames.CacheControl] = "no-cache";
			response.Headers[HeaderNames.Pragma] = "no-cache";
			response.Headers[HeaderNames.Expires] = "-1";
			response.Headers.Remove(HeaderNames.ETag);
			await Task.CompletedTask;
		}

		private async Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			if (exception is BadHttpRequestException &&
			    exception.StackTrace!.Contains(nameof(HttpTelemetryWriter)) &&
			    exception.StackTrace!.Contains("GetHttpContentAsync"))
			{
				// Ignore exceptions which are going through the HttpHeaderTelemetryWriter.GetHttpContentAsync
				// as they must be ignored but may hit this middleware before they are ignored.
				// Also this exception must not modify the HTTP response.
				return;
			}

			logger.LogError(eventId: 0, exception, "An unhandled exception has occurred: " + exception.Message);

			// We can't do anything if the response has already started, just abort.
			if (context.Response.HasStarted)
			{
				logger.LogWarning("The response has already started, the error handler will not be executed.");
				throw exception;
			}

			var originalPath = context.Request.Path;
			if (options.ExceptionHandlingPath.HasValue)
			{
				context.Request.Path = options.ExceptionHandlingPath;
			}

			try
			{
				context.Response.Clear();
				var correlationId = context.Request.GetCorrelationId();
				context.Response.Headers[HttpHeaders.CorrelationId] = correlationId;

				var exceptionHandlerFeature = new ExceptionHandlerFeature
				{
					Error = exception,
					Path = originalPath.Value
				};

				context.Features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);
				context.Features.Set<IExceptionHandlerPathFeature>(exceptionHandlerFeature);
				context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
				context.Response.OnStarting(clearCacheHeadersDelegate, context.Response);

				if (ShouldLogException(exception))
				{
					await LogExceptionAsync(context, exception);
				}

				if (diagnosticSource.IsEnabled("Microsoft.AspNetCore.Diagnostics.HandledException"))
				{
					diagnosticSource.Write("Microsoft.AspNetCore.Diagnostics.HandledException", new
					{
						httpContext = context,
						exception
					});
				}
			}
			catch (Exception exceptionInner)
			{
				// Suppress secondary exceptions, re-throw the original.
				logger.LogError(eventId: 0, exceptionInner,
					"An exception was thrown attempting to execute the error handler.");
			}
			finally
			{
				context.Request.Path = originalPath;
			}
		}

		private async Task HandleUnauthorizedExceptionAsync(HttpContext context, SecurityTokenInvalidIssuerException securityTokenInvalidIssuerException)
		{
			// We can't do anything if the response has already started, just abort.
			if (context.Response.HasStarted)
			{
				logger.LogWarning("The response has already started, the error handler will not be executed.");
				throw securityTokenInvalidIssuerException;
			}

			var originalPath = context.Request.Path;

			try
			{
				var applicationInsights = serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>();
				applicationInsights.TrackTrace($"User authentication failed: {securityTokenInvalidIssuerException.WriteException()}", SeverityLevel.Warning, context.GetTelemetryProperties());
				context.Response.Clear();
				context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
				await Task.CompletedTask;
			}
			catch (Exception exceptionInner)
			{
				// Suppress secondary exceptions, re-throw the original.
				logger.LogError(eventId: 0, exceptionInner, "An exception was thrown attempting to execute the error handler.");
			}
			finally
			{
				context.Request.Path = originalPath;
			}
		}

		private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException validationException)
		{
			// We can't do anything if the response has already started, just abort.
			if (context.Response.HasStarted)
			{
				logger.LogWarning("The response has already started, the error handler will not be executed.");
				throw validationException;
			}

			var originalPath = context.Request.Path;

			try
			{
				context.Response.Clear();
				context.Response.StatusCode = (int) HttpStatusCode.BadRequest;

				var badRequestContent = new BadRequestContract();
				var badRequestErrorContract = new BadRequestErrorContract
				{
					Type = validationException is ValidationSerializationException ? RequestValidationStates.SerializationFailure.ToString() : RequestValidationStates.ValidationFailure.ToString(),
					Message = validationException.Message
				};

				badRequestContent.Errors.Add(badRequestErrorContract);

				var objectResult = new ObjectResult(badRequestContent);
				var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
				await objectResult.ExecuteResultAsync(actionContext);
			}
			catch (Exception exceptionInner)
			{
				// Suppress secondary exceptions, re-throw the original.
				logger.LogError(eventId: 0, exceptionInner, "An exception was thrown attempting to execute the error handler.");
			}
			finally
			{
				context.Request.Path = originalPath;
			}
		}

		private async Task LogExceptionAsync(HttpContext context, Exception exception)
		{
			var customProperties = context.GetTelemetryProperties();
			var applicationInsights = serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>();
			await context.TrackRequestAsync();
			applicationInsights.TrackException(exception, customProperties);
		}

		private static bool ShouldLogException(Exception exception)
		{
			// typically thrown while shutting down the app; no need to log it
			var clientCancelled = exception is ObjectDisposedException || exception is OperationCanceledException;

			// happens when the client disconnects from the tcp connection
			var clientDisconnected = !string.IsNullOrEmpty(exception.Message) && exception.Message.Contains("The client disconnected");
			return !clientDisconnected && !clientCancelled;
		}
	}
}