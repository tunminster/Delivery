using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.Contracts.V1.ModelContracts
{
    [DataContract]
    public class StoreOpeningHourContract
    {
        [DataMember]
        public string DayOfWeek { get; set; }
        
        [DataMember]
        public string Open { get; set; }
        
        [DataMember]
        public string Close { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(DayOfWeek)}: {DayOfWeek.Format()}," +
                   $"{nameof(Open)}: {Open.Format()}," +
                   $"{nameof(Close)}: {Close.Format()}";

        }
    }
}