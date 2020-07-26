using System.Runtime.Serialization;

namespace Delivery.Api.Domain.Contract
{
    [DataContract]
    public class ProductContract
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string ProductName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ProductImage { get; set; }

        [DataMember]
        public string ProductImageUrl { get; set; }

        [DataMember]
        public string UnitPrice { get; set; }

        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public string CategoryName { get; set; }
    }
}