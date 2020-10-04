using System;
using System.Runtime.Serialization;

namespace Delivery.Category.Domain.Contracts
{
    [DataContract]
    public class CategoryContract
    {
        [DataMember]
        public int Id { get; set; }
        
        [DataMember]
        public string CategoryName { get; set; }
        
        [DataMember]
        public string Description { get; set; }
        
        [DataMember]
        public int ParentCategoryId { get; set; }
        
        [DataMember]
        public int Order { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{Id}: {Id.ToString()}";
        }
    }
}