using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.HostedServices.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Blob;
using Polly;

namespace Delivery.Azure.Library.Storage.HostedServices
{
    public class QueueBlobUploadWorkBackgroundService : QueueWorkBackgroundService, IQueueBlobUploadWorkBackgroundService
	{
		private readonly bool isQueueWorkInCallingThread;

		public QueueBlobUploadWorkBackgroundService(IServiceProvider serviceProvider, bool isQueueWorkInCallingThread = false) : base(serviceProvider)
		{
			this.isQueueWorkInCallingThread = isQueueWorkInCallingThread;
		}

		public async Task EnqueueBackgroundWorkAsync(IExecutingRequestContextAdapter executingRequestContextAdapter, CloudBlockBlob blob, Stream stream)
		{
			if (isQueueWorkInCallingThread)
			{
				await QueueWorkAsync(executingRequestContextAdapter, blob, stream);
			}
			else
			{
				var hasEnqueueCompleted = await Policy.HandleResult<bool>(entity => !entity)
					.WaitAndRetryAsync(retryCount: 3, _ => TimeSpan.FromSeconds(value: 1))
					.ExecuteAsync(async () =>
					{
						return await Task.Run( () => EnqueueBackgroundWork(_ => QueueWorkAsync(executingRequestContextAdapter, blob, stream)));
					});

				if (!hasEnqueueCompleted)
				{
					throw new InvalidOperationException($"{blob.Name} has failed to enqueue at the {nameof(QueueBlobUploadWorkBackgroundService)}")
						.WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
				}
			}
		}

		private async Task QueueWorkAsync(IExecutingRequestContextAdapter executingRequestContextAdapter, CloudBlockBlob blob, Stream stream)
		{
			try
			{
				var dependencyName = blob.Container.Name;
				await new DependencyMeasurement(ServiceProvider)
					.ForDependency(dependencyName, MeasuredDependencyType.AzureBlob, blob.Name, blob.Uri.AbsoluteUri)
					.WithContextualInformation(executingRequestContextAdapter.GetTelemetryProperties())
					.TrackAsync(async () =>
					{
						// Define the BlobRequestOptions on the upload
						BlobRequestOptions options = new()
						{
							ParallelOperationThreadCount = 8,
							DisableContentMD5Validation = true,
							StoreBlobContentMD5 = false
						};

						await blob.UploadFromStreamAsync(stream, accessCondition: null, options, operationContext: null)
							.ContinueWith(async task =>
							{
								await stream.DisposeAsync();

								if (task.Exception != null)
								{
									throw task.Exception;
								}
							}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
					});
			}
			catch (Exception exception)
			{
				ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(exception, executingRequestContextAdapter.GetTelemetryProperties());
			}
		}
	}
}