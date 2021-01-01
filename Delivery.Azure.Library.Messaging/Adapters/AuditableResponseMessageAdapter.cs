using System;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Azure.Library.Sharding.Adapters;

namespace Delivery.Azure.Library.Messaging.Adapters
{
    public class AuditableResponseMessageAdapter<TPayloadIn, TPayloadOut>
		where TPayloadIn : class, new()
		where TPayloadOut : class, new()
	{
		private readonly TPayloadIn payloadIn;
		private readonly TPayloadOut payloadOut;
		private readonly ExecutingRequestContext requestContext;
		private readonly AuditableResponseMessage<TPayloadIn, TPayloadOut> message;

		public AuditableResponseMessageAdapter(AuditableResponseMessage<TPayloadIn, TPayloadOut> auditableResponseMessage)
		{
			message = auditableResponseMessage ?? throw new InvalidOperationException($"{nameof(auditableResponseMessage)} must be provided: {auditableResponseMessage.Format()} Found: {message.ConvertToJson()}");
			payloadIn = auditableResponseMessage.PayloadIn ?? throw new InvalidOperationException($"{nameof(auditableResponseMessage.PayloadIn)} must be provided: {auditableResponseMessage.PayloadIn.Format()} Found: {message.ConvertToJson()}");
			payloadOut = auditableResponseMessage.PayloadOut ?? throw new InvalidOperationException($"{nameof(auditableResponseMessage.PayloadOut)} must be provided: {auditableResponseMessage.PayloadOut.Format()} Found: {message.ConvertToJson()}");
			requestContext = auditableResponseMessage.RequestContext ?? throw new InvalidOperationException($"{nameof(auditableResponseMessage.RequestContext)} must be provided: {auditableResponseMessage.RequestContext.Format()} Found: {message.ConvertToJson()}");

			if (auditableResponseMessage.RequestContext.AuthenticatedUser == null)
			{
				throw new NotSupportedException($"{nameof(auditableResponseMessage.RequestContext.AuthenticatedUser)} must be set. Found: {message.ConvertToJson()}");
			}

			if (string.IsNullOrEmpty(auditableResponseMessage.RequestContext.AuthenticatedUser?.UserEmail))
			{
				throw new NotSupportedException($"{nameof(auditableResponseMessage.RequestContext.AuthenticatedUser.UserEmail)} must be set. Found: {message.ConvertToJson()}");
			}
		}

		public AuditableResponseMessage<TPayloadIn, TPayloadOut> GetMessage()
		{
			return message;
		}

		public TPayloadIn GetPayloadIn()
		{
			return payloadIn;
		}

		public TPayloadOut GetPayloadOut()
		{
			return payloadOut;
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