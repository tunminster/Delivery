namespace Delivery.Azure.Library.Contracts.Interfaces.V1.Entities
{
    /// <summary>
    ///     Uniquely locates an entity by its id
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        ///     The primary key of the entity
        /// </summary>
        int Id { get; set; }

        /// <summary>
        ///     The unique identifier which can be shared externally
        /// </summary>
        string ExternalId { get; set; }
    }
}