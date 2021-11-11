using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.Contracts.V1.ModelContracts
{
    /// <summary>
    ///  Store opening hour contract
    /// </summary>
    public class StoreOpeningHourContract
    {
        /// <summary>
        ///  Day of week
        /// <example>{{dayOfWeek}}</example>
        /// </summary>
        public string DayOfWeek { get; set; } = string.Empty;

        /// <summary>
        ///  Open
        /// <example>{{open}}</example>
        /// </summary>
        public string Open { get; set; } = string.Empty;

        /// <summary>
        ///  Close
        /// <example>{{close}}</example>
        /// </summary>
        public string Close { get; set; } = string.Empty;

        /// <summary>
        ///  Timezone
        /// <example>{{timeZone}}</example>
        /// </summary>
        public string TimeZone { get; set; } = string.Empty;
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(DayOfWeek)}: {DayOfWeek.Format()}," +
                   $"{nameof(Open)}: {Open.Format()}," +
                   $"{nameof(Close)}: {Close.Format()}" +
                    $"{nameof(TimeZone)}: {TimeZone.Format()}";

        }
    }
}