using System.Runtime.Serialization;

namespace Delivery.StripePayment.Domain.Contracts.Enums
{
    /// <summary>
    ///  An enum represents stripe country code
    /// </summary>
    [DataContract]
    public enum StripeCountryCode
    {
        [EnumMember] None = 0,
        /// <summary>
        /// United States
        /// </summary>
        [EnumMember] Us = 1,
        /// <summary>
        /// United Kingdom
        /// </summary>
        [EnumMember] Gb = 2,
        /// <summary>
        /// Switzerland
        /// </summary>
        [EnumMember] Ch = 3,
        /// <summary>
        /// Sweden
        /// </summary>
        [EnumMember] Se = 4,
        /// <summary>
        /// Spain
        /// </summary>
        [EnumMember] Es = 5,
        /// <summary>
        /// Singapore
        /// </summary>
        [EnumMember] Sg = 6,
        /// <summary>
        /// Japan
        /// </summary>
        [EnumMember] Jp = 7
        
        
    }
}