using System.Runtime.Serialization;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts;

namespace Delivery.Azure.Library.Contracts.Contracts
{
    [DataContract]
    public class PartitionedContract : IPartitionedContract
    {
        /// <summary>
        ///     Current partition number
        /// </summary>
        [DataMember]
        public int? CurrentPartitionNumber { get; set; }

        /// <summary>
        ///     Total partition count
        /// </summary>
        [DataMember]
        public int? TotalPartitionCount { get; set; }
    }
}