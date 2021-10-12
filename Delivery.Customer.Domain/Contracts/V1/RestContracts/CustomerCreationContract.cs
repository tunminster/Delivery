using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Customer.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Customer creation contract
    /// </summary>
    public record CustomerCreationContract
    {
        /// <summary>
        ///  Identity id
        /// </summary>
        public string IdentityId { get; init; } = string.Empty;

        /// <summary>
        ///  Username
        /// </summary>
        public string Username { get; init; } = string.Empty;

        /// <summary>
        ///  First name
        /// </summary>
        public string FirstName { get; init; } = string.Empty;

        /// <summary>
        ///  Lastname
        /// </summary>
        public string LastName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Contact number
        /// </summary>
        public string ContactNumber { get; init; } = string.Empty;
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(IdentityId)}: {IdentityId.Format()}," +
                   $"{nameof(FirstName)}: {FirstName.Format()}," +
                   $"{nameof(LastName)}: {LastName.Format()}," +
                   $"{nameof(ContactNumber)}: {ContactNumber.Format()}," +
                   $"{nameof(Username)} : {Username.Format()}";

        }
    }
}