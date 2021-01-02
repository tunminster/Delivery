using CloudNative.CloudEvents;
using Delivery.Azure.Library.Messaging.Contracts;
using Delivery.Azure.Library.Messaging.Extensions;

namespace Delivery.Azure.Library.Messaging.ServiceBus.CloudEvents
{
    public static class CloudEventMessageContractAdapter
    {
        public static CloudEventMessageContract Convert(this CloudEvent cloudEvent)
        {
            var cloudEventMessageContract = new CloudEventMessageContract
            {
                Specversion = cloudEvent.SpecVersion.ToString().Replace("V", string.Empty),
                MessageId = cloudEvent.Extension<CloudEventMessageExtensions>().MessageId,
                CorrelationId = cloudEvent.Extension<CloudEventMessageExtensions>().CorrelationId,
                Shard = cloudEvent.Extension<CloudEventMessageExtensions>().Shard,
                Ring = cloudEvent.Extension<CloudEventMessageExtensions>().Ring,
                Type = cloudEvent.Type,
                Source = cloudEvent.Source.AbsoluteUri,
                Id = cloudEvent.Id,
                Time = cloudEvent.Time.GetValueOrDefault().ToUniversalTime().ToString("O"),
                Datacontenttype = cloudEvent.DataContentType.ToString(),
                Data = cloudEvent.Data
            };

            return cloudEventMessageContract;
        }
    }
}