using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using ValidationException = FluentValidation.ValidationException;

namespace Delivery.Domain.CommandHandlers
{
    public abstract class CommandHandler<TCommand, TResult>
    {
        protected IServiceProvider ServiceProvider { get; }

        protected ValidationResult ValidationResult { get; } = new();
        
        protected  IExecutingRequestContextAdapter ExecutingRequestContext { get;  }
        
        public TCommand Command { get; set; }

        protected CommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            ServiceProvider = serviceProvider;
            ExecutingRequestContext = executingRequestContextAdapter;
        }

        public async Task<TResult> ExecuteAsync(TCommand command)
        {
            ExceptionDispatchInfo? exceptionDispatchInfo = null;
            try
            {
                TrackHandlerTrace($"{typeof(TCommand).Name}", "initiated",
                    ExecutingRequestContext.GetTelemetryProperties());
                var result = await HandleAsync(command);
                TrackHandlerTrace($"{typeof(TCommand).Name}", "handled",
                    ExecutingRequestContext.GetTelemetryProperties());
                return result;
            }
            catch (ValidationException validationException)
            {
                ValidationResult.Errors.AddRange(validationException.Errors.Any()
                    ? validationException.Errors
                    : new List<ValidationFailure> {new(typeof(TCommand).Name, validationException.Message)});
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException validationException)
            {
                ValidationResult.Errors.Add(new ValidationFailure(typeof(TCommand).Name, validationException.Message));
            }
            catch (Exception exception)
            {
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(exception, ExecutingRequestContext.GetTelemetryProperties());
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(
                    new CommandHandlerException($"Failed executing handler {GetType().FullName}", exception));

                return Next(exceptionDispatchInfo);
            }

            return Next(null);
        }
        
        protected abstract Task<TResult> HandleAsync(TCommand command);

        private void TrackHandlerTrace(string command, string status, Dictionary<string, string> customerProperties)
        {
            ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"{command} {status}", SeverityLevel.Information, customerProperties);
        }

        protected TResult Next(ExceptionDispatchInfo? exceptionDispatchInfo)
        {
            // to do before throwing error

            if (exceptionDispatchInfo != null)
            {
                exceptionDispatchInfo.Throw();
            }

            throw new InvalidOperationException($"{GetType().FullName} is not able to executed.").WithTelemetry(ExecutingRequestContext.GetTelemetryProperties());
        }

    }
}