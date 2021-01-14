using System.Runtime.Serialization;

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
        public string AddressLine1 { get; set; }
        
        [DataMember]
        public string AddressLine2 { get; set; }
        
        [DataMember]
        public string City { get; set; }
        
        [DataMember]
        public string County { get; set; }
        
        [DataMember]
        public string Country { get; set; }
        
        
    }
}