using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts;
using Delivery.Azure.Library.Sharding.Adapters;

namespace Delivery.Azure.Library.Communications.SendGrid.CommandHandlers
{
    public record EmailNotificationCommand(SendGridEmailCreationContract SendGridEmailCreationContract, SendGridEmailNotificationStatusContract SendGridEmailNotificationStatusContract);
    public abstract class EmailNotificationCommandHandler
    {
        protected EmailNotificationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            ServiceProvider = serviceProvider;
            ExecutingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        protected IServiceProvider ServiceProvider { get; }
        
        protected IExecutingRequestContextAdapter ExecutingRequestContextAdapter { get; }

        public abstract Task HandleAsync(EmailNotificationCommand command);
    }
}