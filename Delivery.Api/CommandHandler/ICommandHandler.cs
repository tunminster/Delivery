using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Api.Entities;

namespace Delivery.Api.CommandHandler
{
    public interface ICommandHandler<TCommand>
    {
        //void Handle(TCommand command);
        Task Handle(TCommand command);
    }
}
