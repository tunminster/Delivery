using Delivery.Domain.QueryHandlers;
using Stripe;

namespace Delivery.StripePayment.Domain.QueryHandlers.Stripe.ConnectAccounts
{
    public class ConnectAccountGetQuery : IQuery<StripeList<Account>>
    {
        public ConnectAccountGetQuery(int limit, string endingBefore, string startingAfter)
        {
            Limit = limit;
            EndingBefore = endingBefore;
            StartingAfter = startingAfter;
        }
        public int Limit { get; set; }
        
        public string EndingBefore { get; set; }
        
        public string StartingAfter { get; set; }
    }
}