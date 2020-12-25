using System;
using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Connection.Managers.Exceptions
{
    [Serializable]
    public class ConnectionFailedException : Exception
    {
        public ConnectionFailedException(string entityName, string connectionType, string connectionStringName) : this(entityName, connectionType, connectionStringName, ComposeExceptionMessage(entityName, connectionType, connectionStringName))
		{
		}

		public ConnectionFailedException(string entityName, string connectionType, string connectionStringName, string message) : base(message)
		{
			EntityName = entityName;
			ConnectionType = connectionType;
			ConnectionStringName = connectionStringName;
		}

		public ConnectionFailedException(string entityName, string connectionType, string connectionStringName, Exception innerException) : this(entityName, connectionType, connectionStringName, ComposeExceptionMessage(entityName, connectionType, connectionStringName), innerException)
		{
		}

		public ConnectionFailedException(string entityName, string connectionType, string connectionStringName, string message, Exception innerException) : base(message, innerException)
		{
			EntityName = entityName;
			ConnectionType = connectionType;
			ConnectionStringName = connectionStringName;
		}

		protected ConnectionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			ConnectionStringName = info.GetString(ConnectionStringName ?? string.Empty);
			ConnectionType = info.GetString(ConnectionType ?? string.Empty);
			EntityName = info.GetString(EntityName ?? string.Empty);
		}

		/// <summary>
		///     Name of the connection string to use when connecting
		/// </summary>
		[DataMember]
		public string? ConnectionStringName { get; }

		/// <summary>
		///     Type of connection
		/// </summary>
		[DataMember]
		public string? ConnectionType { get; }

		/// <summary>
		///     Name of the entity to connect to ie. queue name
		/// </summary>
		[DataMember]
		public string? EntityName { get; }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(ConnectionStringName), ConnectionStringName);
			info.AddValue(nameof(ConnectionType), ConnectionType);
			info.AddValue(nameof(EntityName), EntityName);

			base.GetObjectData(info, context);
		}

		private static string ComposeExceptionMessage(string entityName, string connectionType, string connectionStringName)
		{
			return $"Could not create a connection for entity '{entityName}' from '{connectionType}' for connection string '{connectionStringName}'";
		}
    }
}