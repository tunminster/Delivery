using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Api.Entities;

namespace Delivery.Api.CommandHandler
{
    public interface ICommandHandler<TCommand, TResult>
    {
        //void Handle(TCommand command);
        Task<TResult> Handle(TCommand command);
    }
}
