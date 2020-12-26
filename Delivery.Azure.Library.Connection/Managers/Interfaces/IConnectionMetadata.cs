namespace Delivery.Azure.Library.Connection.Managers.Interfaces
{
    public interface IConnectionMetadata
    {
        /// <summary>
        ///     Current partition that is being used
        /// </summary>
        int Partition { get; }

        /// <summary>
        ///     Name of the secret that contains the connection string
        /// </summary>
        string SecretName { get; }

        /// <summary>
        ///     Name of the entity
        /// </summary>
        string EntityName { get; }
    }
}