using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Category.Domain.Contracts.V1.RestContracts
{
    [DataContract]
    public class CategoryCreationContract
    {
        [DataMember]
        public string Id { get; set; }
        
        [DataMember]
        public string CategoryName { get; set; }
        
        [DataMember]
        public string Description { get; set; }
        
        [DataMember]
        public int ParentCategoryId { get; set; }
        
        [DataMember]
        public string StoreId { get; set; }
        
        [DataMember]
        public int Order { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Id)}: {Id.Format()}," +
                   $"{nameof(CategoryName)}: {CategoryName.Format()}," +
                   $"{nameof(Description)}: {Description.Format()}," +
                   $"{nameof(ParentCategoryId)}: {ParentCategoryId.Format()}," +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(Order)}: {Order.Format()};";
        }
    }
}