using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Nest;

namespace Delivery.Store.Domain.Contracts.V1.ModelContracts
{
    /// <summary>
    ///  A contract for store
    /// </summary>
    [DataContract]
    public class StoreContract
    {
        [DataMember]
        public string StoreId { get; set; }
        
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
        public string StoreType { get; set; }
        
        [DataMember]
        public string StorePaymentAccountNumber { get; set; }
        
        [DataMember]
        public List<StoreOpeningHourContract> StoreOpeningHours { get; set; }
        
        [DataMember]
        [GeoPoint(Name = "location")]
        public GeoLocation Location { get; set; }
        
        [DataMember]
        public double Distance { get; set; }
        
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(StoreName)}: {StoreName.Format()}," +
                   $"{nameof(ImageUri)}: {ImageUri.Format()}," +
                   $"{nameof(AddressLine1)}: {AddressLine1.Format()}," +
                   $"{nameof(AddressLine2)}: {AddressLine2.Format()}," +
                   $"{nameof(City)}: {City.Format()}," +
                   $"{nameof(County)}: {County.Format()}," +
                   $"{nameof(Country)}: {Country.Format()}," +
                   $"{nameof(PostalCode)}: {PostalCode.Format()}," +
                   $"{nameof(Location)}: {Location.Format()}," +
                   $"{nameof(StoreType)} : {StoreType.Format()}";

        }
    }
}