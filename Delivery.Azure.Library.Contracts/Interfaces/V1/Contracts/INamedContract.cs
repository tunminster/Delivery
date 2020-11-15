namespace Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts
{
    /// <summary>
    ///     All objects which expose a named representation should implement this interface
    /// </summary>
    public interface INamedContract
    {
        /// <summary>
        ///     The display name of the entity
        /// </summary>
        string? Name { get; set; }
    }
}