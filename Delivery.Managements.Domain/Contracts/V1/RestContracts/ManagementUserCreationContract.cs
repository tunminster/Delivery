namespace Delivery.Managements.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Management user contract
    /// </summary>
    public record ManagementUserCreationContract
    {
        /// <summary>
        ///  User full name
        /// </summary>
        /// <example>{{fullName}}</example>
        public string FullName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Email address
        /// </summary>
        /// <example>{{emailAddress}}</example>
        public string EmailAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Password
        /// </summary>
        /// <example>{{password}}</example>
        public string Password { get; init; } = string.Empty;
        
        /// <summary>
        ///  Confirm password
        /// </summary>
        /// <example>{{confirmPassword}}</example>
        public string ConfirmPassword { get; init; } = string.Empty;
    }
}