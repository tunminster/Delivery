namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations
{
    /// <summary>
    ///  Store user creation contract
    /// </summary>
    public record StoreUserCreationContract
    {
        /// <summary>
        ///  Email address
        /// <example>{{emailAddress}}</example>
        /// </summary>
        public string EmailAddress { get; init; } = string.Empty;

        /// <summary>
        ///  Password
        /// <example>{{password}}</example>
        /// </summary>
        public string Password { get; init; } = string.Empty;

        /// <summary>
        ///  Confirm password
        /// <example>{{confirmPassword}}</example>
        /// </summary>
        public string ConfirmPassword { get; init; } = string.Empty;
    }
}