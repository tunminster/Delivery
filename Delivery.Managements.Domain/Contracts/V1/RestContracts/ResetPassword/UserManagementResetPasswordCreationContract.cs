namespace Delivery.Managements.Domain.Contracts.V1.RestContracts.ResetPassword
{
    /// <summary>
    ///  User management reset password creation contract
    /// </summary>
    public record UserManagementResetPasswordCreationContract
    {
        /// <summary>
        ///  Email
        /// </summary>
        /// <example>{{email}}</example>
        public string Email { get; init; } = string.Empty;
        
        /// <summary>
        ///  Otp code
        /// </summary>
        /// <example>{{code}}</example>
        public string Code { get; init; } = string.Empty;

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