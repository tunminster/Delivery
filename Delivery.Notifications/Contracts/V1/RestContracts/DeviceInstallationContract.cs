using System.Collections;
using System.Collections.Generic;

namespace Delivery.Notifications.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Register device contract
    /// </summary>
    public record DeviceInstallationContract
    {
        /// <summary>
        ///  Installation id
        /// <example>{{installationId}}</example>
        /// </summary>
        public string InstallationId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Platform
        /// <example>{{platform}}</example>
        /// </summary>
        public string Platform { get; init; } = string.Empty;
        
        /// <summary>
        ///  Push channel
        /// <example>{{pushChannel}}</example>
        /// </summary>
        public string PushChannel { get; init; } = string.Empty;
        
        /// <summary>
        ///  Tags
        /// <example>{{tags}}</example>
        /// </summary>
        public List<string> Tags { get; init; } = new();
    }
}