using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Models;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverAssignment
{
    public record DriverByNearestLocationQuery : IQuery<List<DriverModel>>
    {
        /// <summary>
        ///  Service area latitude
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        ///  Service area longitude
        /// </summary>
        public double Longitude { get; set; }
    }
    
    public class DriverByNearestLocationQueryHandler : IQueryHandler<DriverByNearestLocationQuery, List<DriverModel>>
    {
        public Task<List<DriverModel>> Handle(DriverByNearestLocationQuery query)
        {
            throw new System.NotImplementedException();
        }
    }
}