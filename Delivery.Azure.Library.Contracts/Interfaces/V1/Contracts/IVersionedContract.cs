namespace Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts
{
    /// <summary>
    ///     Allows objects to be serialized/deserialized by their correct versions
    /// </summary>
    public interface IVersionedContract
    {
        /// <summary>
        ///     The version of the entity which matches the data store representation
        /// </summary>
        int Version { get; }
    }
}