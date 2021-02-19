using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations
{
    /// <summary>
    ///  A contract to receive a store
    /// </summary>
    [DataContract]
    public class StoreCreationContract
    {
        [DataMember]
        public string StoreName { get; set; }
        
        [DataMember]
        public string ImageUri { get; set; }
        
        [DataMember]
        public string AddressLine1 { get; set; }
        
        [DataMember]
        public string AddressLine2 { get; set; }
        
        [DataMember]
        public string City { get; set; }
        
        [DataMember]
        public string County { get; set; }
        
        [DataMember]
        public string Country { get; set; }
        
        [DataMember]
        public string PostalCode { get; set; }
        
        [DataMember]
        public string StoreTypeId { get; set; }
        
        [DataMember]
        public List<StoreOpeningHourCreationContract> StoreOpeningHours { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreName)}: {StoreName.Format()}," +
                   $"{nameof(ImageUri)}: {ImageUri.Format()}," +
                   $"{nameof(AddressLine1)}: {AddressLine1.Format()}," +
                   $"{nameof(AddressLine2)}: {AddressLine2.Format()}," +
                   $"{nameof(City)}: {City.Format()}," +
                   $"{nameof(County)}: {County.Format()}," +
                   $"{nameof(Country)}: {Country.Format()}," +
                   $"{nameof(PostalCode)}: {PostalCode.Format()}," +
                   $"{nameof(StoreOpeningHours)}: {StoreOpeningHours.Format()}," +
                   $"{nameof(StoreTypeId)} : {StoreTypeId.Format()}";

        }
    }
}