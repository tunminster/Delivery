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
        /// <summary>
        ///  Store name
        /// </summary>
        ///<example>Buenos Aires</example>
        [DataMember]
        public string StoreName { get; set; }
        
        /// <summary>
        ///  Image uri
        /// </summary>
        /// <example>https://www.imageurl.com</example>
        [DataMember]
        public string ImageUri { get; set; }
        
        /// <summary>
        ///  Address line 1
        /// </summary>
        /// <example>Address line 1</example>
        [DataMember]
        public string AddressLine1 { get; set; }
        
        /// <summary>
        ///  Address Line 2
        /// </summary>
        /// <example>Address line 2</example>
        [DataMember]
        public string AddressLine2 { get; set; }
        
        /// <summary>
        ///  City
        /// </summary>
        /// <example>New york city</example>
        [DataMember]
        public string City { get; set; }
        
        /// <summary>
        ///  County
        /// </summary>
        /// <example>New york</example>
        [DataMember]
        public string County { get; set; }
        
        /// <summary>
        ///  Country
        /// </summary>
        /// <example>United States</example>
        [DataMember]
        public string Country { get; set; }
        
        /// <summary>
        ///  Postalcode
        /// </summary>
        /// <example>95012</example>
        [DataMember]
        public string PostalCode { get; set; }
        
        /// <summary>
        ///  StoreTypeId
        /// </summary>
        /// <example>da12345</example>
        [DataMember]
        public string StoreTypeId { get; set; }
        
        /// <summary>
        ///  Payment account number
        /// </summary>
        /// <example>acct_1IZcerRGV3DhAqtX</example>
        [DataMember]
        public string PaymentAccountNumber { get; set; }
        
        /// <summary>
        ///  Store opening hours
        /// </summary>
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