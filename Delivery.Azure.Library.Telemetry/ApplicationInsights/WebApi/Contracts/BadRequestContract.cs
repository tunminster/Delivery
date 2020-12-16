using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts
{
    /// <summary>
    ///     Contains error information for a BadRequest (400) response.
    /// </summary>
    [DataContract]
    public class BadRequestContract
    {
        /// <summary>
        ///     Gets the list of errors.
        /// </summary>
        [DataMember]
        public List<BadRequestErrorContract> Errors { get; set; } = new List<BadRequestErrorContract>();

        public override string ToString()
        {
            return $"{nameof(BadRequestContract)} - {nameof(BadRequestErrorContract)}: {Errors.Format()}";
        }
    }
}