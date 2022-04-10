using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverOnBoardingEmail
{
    public record DriverOnBoardingEmailCommand(string DriverEmail);
    public class DriverOnBoardingEmailCommandHandler : ICommandHandler<DriverOnBoardingEmailCommand>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverOnBoardingEmailCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public Task Handle(DriverOnBoardingEmailCommand command)
        {
             throw new System.NotImplementedException();
        }
    }
}

