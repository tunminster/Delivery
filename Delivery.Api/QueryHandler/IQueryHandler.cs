using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Api.Entities;


namespace Delivery.Api.QueryHandler
{
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        TResult Handle(TQuery query);
    }
}
