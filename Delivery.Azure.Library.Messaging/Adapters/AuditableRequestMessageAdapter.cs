using System;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Azure.Library.Sharding.Adapters;

namespace Delivery.Azure.Library.Messaging.Adapters
{
    public class AuditableRequestMessageAdapter<TPayloadIn>
        where TPayloadIn : class, new()
    {
        private readonly TPayloadIn payloadIn;
        private readonly ExecutingRequestContext requestContext;
        private readonly AuditableRequestMessage<TPayloadIn> message;

        public AuditableRequestMessageAdapter(AuditableRequestMessage<TPayloadIn> auditableRequestMessage)
        {
            message = auditableRequestMessage ?? throw new InvalidOperationException($"{nameof(auditableRequestMessage)} must be provided: {auditableRequestMessage.Format()} Found: {message.ConvertToJson()}");
            payloadIn = auditableRequestMessage.PayloadIn ?? throw new InvalidOperationException($"{nameof(auditableRequestMessage.PayloadIn)} must be provided: {auditableRequestMessage.PayloadIn.Format()} Found: {message.ConvertToJson()}");
            requestContext = auditableRequestMessage.RequestContext ?? throw new InvalidOperationException($"{nameof(auditableRequestMessage.RequestContext)} must be provided: {auditableRequestMessage.RequestContext.Format()} Found: {message.ConvertToJson()}");

            if (auditableRequestMessage.RequestContext.AuthenticatedUser == null)
            {
                throw new NotSupportedException($"{nameof(auditableRequestMessage.RequestContext.AuthenticatedUser)} must be set. Found: {message.ConvertToJson()}");
            }

            if (string.IsNullOrEmpty(auditableRequestMessage.RequestContext.AuthenticatedUser?.UserEmail))
            {
                throw new NotSupportedException($"{nameof(auditableRequestMessage.RequestContext.AuthenticatedUser.UserEmail)} must be set. Found: {message.ConvertToJson()}");
            }
        }

        public AuditableRequestMessage<TPayloadIn> GetMessage()
        {
            return message;
        }

        public TPayloadIn PayloadIn()
        {
            return payloadIn;
        }

        public ExecutingRequestContext GetExecutingRequestContext()
        {
            return requestContext;
        }

        public IExecutingRequestContextAdapter GetExecutingRequestContextAdapter()
        {
            return new ExecutingRequestContextAdapter(requestContext);
        }
    }
}