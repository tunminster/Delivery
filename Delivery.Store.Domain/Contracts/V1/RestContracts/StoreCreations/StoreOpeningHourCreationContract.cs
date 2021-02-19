using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations
{
    [DataContract]
    public class StoreOpeningHourCreationContract : StoreOpeningHourContract
    {
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(DayOfWeek)}: {DayOfWeek.Format()}," +
                   $"{nameof(Open)}: {Open.Format()}," +
                   $"{nameof(Close)}: {Close.Format()}";

        }
    }
}