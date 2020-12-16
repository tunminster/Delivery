using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Configuration.Environments.Enums
{
    /// <summary>
    ///     Allows extending the <see cref="Environments " /> enum while maintaining compatibility
    /// </summary>
    [DataContract]
    public enum RuntimeEnvironment
    {
        [EnumMember] None,
        [EnumMember] Fwk, // local framework development
        [EnumMember] Dev, // local platform development
        [EnumMember] Sbx, // deployed sandbox environment
        [EnumMember] Npd, // deployed non-prod environment
        [EnumMember] Prd // deployed prd environment
    }
}