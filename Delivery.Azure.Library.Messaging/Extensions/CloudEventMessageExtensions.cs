using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;

namespace Delivery.Azure.Library.Messaging.Extensions
{
    public class CloudEventMessageExtensions : ICloudEventExtension
    {
        private IDictionary<string, object> attributes = new Dictionary<string, object>();
        private const string MessageIdAttributeName = "messageId";
        private const string CorrelationIdAttributeName = "correlationId";
        private const string ShardAttributeName = "shard";
        private const string RingAttributeName = "ring";

        public string MessageId
        {
            get => attributes[MessageIdAttributeName] as string;
            set => attributes[MessageIdAttributeName] = value!;
        }

        public string CorrelationId
        {
            get => attributes[CorrelationIdAttributeName] as string;
            set => attributes[CorrelationIdAttributeName] = value!;
        }

        public string Shard
        {
            get => attributes[ShardAttributeName] as string;
            set => attributes[ShardAttributeName] = value!;
        }

        public int Ring
        {
            get => (int) (attributes[RingAttributeName] ?? -1);
            set => attributes[RingAttributeName] = value;
        }
        
        public void Attach(CloudEvent cloudEvent)
        {
            var eventAttributes = cloudEvent.GetAttributes();
            if (attributes == eventAttributes)
            {
                return;
            }

            foreach (var attribute in attributes)
            {
                if (attribute.Value != null)
                {
                    eventAttributes[attribute.Key] = attribute.Value;
                }
            }

            attributes = eventAttributes;
        }

        public bool ValidateAndNormalize(string key, ref dynamic value)
        {
            switch (key)
            {
                case MessageIdAttributeName:
                    if (value is string)
                    {
                        return true;
                    }

                    throw new InvalidOperationException("The value must be string.");
                case CorrelationIdAttributeName:
                    if (value is string)
                    {
                        return true;
                    }

                    throw new InvalidOperationException("The value must be string.");
                case ShardAttributeName:
                    if (value is string)
                    {
                        return true;
                    }

                    throw new InvalidOperationException("The value must be string.");
                case RingAttributeName:
                    if (value is int)
                    {
                        return true;
                    }

                    throw new InvalidOperationException("The value must be integer.");
                default:
                    return false;
            }
        }

        public Type GetAttributeType(string name)
        {
            switch (name)
            {
                case MessageIdAttributeName:
                case CorrelationIdAttributeName:
                case ShardAttributeName:
                    return typeof(string);
                case RingAttributeName:
                    return typeof(int);
                default:
                    return null;
            }
        }
    }
}