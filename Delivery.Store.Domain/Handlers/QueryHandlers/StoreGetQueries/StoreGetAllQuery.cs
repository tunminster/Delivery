using System.Collections.Generic;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries
{
    public class StoreGetAllQuery : IQuery<List<StoreContract>>
    {
        public StoreGetAllQuery(int numberOfObjectPerPage, int pageNumber)
        {
            NumberOfObjectPerPage = numberOfObjectPerPage;
            PageNumber = pageNumber;
        }
        
        public int NumberOfObjectPerPage { get;  }
        
        public int PageNumber { get;  }

    }
}