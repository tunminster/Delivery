namespace Delivery.Azure.Library.Contracts.Interfaces.V1.Entities
{
    /// <summary>
    ///     All entities which have activation/deletion implement this interface
    /// </summary>
    public interface ISoftDeleteEntity
    {
        /// <summary>
        ///     Soft deletion; if inactive then entity is considered deleted
        /// </summary>
        bool IsDeleted { get; set; }
    }
}