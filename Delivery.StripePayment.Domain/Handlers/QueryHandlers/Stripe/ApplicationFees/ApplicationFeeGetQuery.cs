using Delivery.Domain.QueryHandlers;
using Stripe;

namespace Delivery.StripePayment.Domain.Handlers.QueryHandlers.Stripe.ApplicationFees
{
    public class ApplicationFeeGetQuery : IQuery<StripeList<ApplicationFee>>
    {
        public ApplicationFeeGetQuery(int limit, string startingAfter, string endingBefore)
        {
            Limit = limit;
            StartingAfter = startingAfter;
            EndingBefore = endingBefore;
        }
        public int Limit { get; }
        public string EndingBefore { get; }
        
        public string StartingAfter { get; }
    }
}