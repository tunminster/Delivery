namespace Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts
{
    /// <summary>
    ///     All objects which requires partitioning should implement this interface
    /// </summary>
    public interface IPartitionedContract
    {
        /// <summary>
        ///     Current partition number
        /// </summary>
        int? CurrentPartitionNumber { get; set; }

        /// <summary>
        ///     Total partition count
        /// </summary>
        int? TotalPartitionCount { get; set; }
    }
}