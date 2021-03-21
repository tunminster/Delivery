using Delivery.Domain.QueryHandlers;
using Stripe;

namespace Delivery.StripePayment.Domain.Handlers.QueryHandlers.Stripe.ConnectAccounts
{
    public class ConnectAccountGetQuery : IQuery<StripeList<Account>>
    {
        public ConnectAccountGetQuery(int limit, string endingBefore, string startingAfter)
        {
            Limit = limit;
            EndingBefore = endingBefore;
            StartingAfter = startingAfter;
        }
        public int Limit { get; }
        
        public string EndingBefore { get; }
        
        public string StartingAfter { get; }
    }
}