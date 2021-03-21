using System.Collections.Generic;
using Delivery.Azure.Library.Storage.Cosmos.Contracts;
using Delivery.Domain.QueryHandlers;
using Delivery.StripePayment.Domain.Contracts.Models;

namespace Delivery.StripePayment.Domain.Handlers.QueryHandlers.Stripe.AccountLinks
{
    public class AccountLinkGetQuery : IQuery<List<DocumentContract<StripeAccountContract>?>>
    {
        public AccountLinkGetQuery(string partitionKey, string email)
        {
            PartitionKey = partitionKey;
            Email = email;
        }
        public string PartitionKey { get;  }
        public string Email { get; }
    }
}