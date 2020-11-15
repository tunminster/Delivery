using System.Runtime.Serialization;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts;
using Delivery.Azure.Library.Core.Extensions;

namespace Delivery.Azure.Library.Contracts.Contracts
{
    /// <summary>
    ///     Allows to pass the authenticated user details from the api to downstream receivers.
    /// </summary>
    [DataContract]
    public class AuthenticatedUserContract : IVersionedContract
    {
        /// <summary>
        ///     The email address of the actually-authenticated user. Must not be a tech user.
        /// </summary>
        [DataMember]
        public string? UserEmail { get; set; }

        /// <summary>
        ///     The role of the user.
        /// </summary>
        [DataMember]
        public string? Role { get; set; }

        /// <summary>
        ///     The key of the tenant which the user belongs to.
        /// </summary>
        [DataMember]
        public string? ShardKey { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}: {nameof(UserEmail)}: {UserEmail.Format()}, {nameof(Role)}: {Role.Format()}, {nameof(ShardKey)}: {ShardKey.Format()}";
        }

        public int Version { get; } = 1;
    }
}