using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Core.Guards;

namespace Delivery.Azure.Library.Connection.Managers
{
    public class ConnectionMetadata : IConnectionMetadata
    {
        public ConnectionMetadata(string entityName, string secretName, int partition)
        {
            Guard.Against(partition < 0, nameof(partition));

            Partition = partition;
            EntityName = entityName;
            SecretName = secretName;
        }
        
        public int Partition { get; }
        public string SecretName { get; }
        public string EntityName { get; }
    }
}