namespace Delivery.Managements.Domain.Contracts.V1.RestContracts.ResetPassword
{
    /// <summary>
    ///  User reset password request contract
    /// </summary>
    public record UserManagementResetPasswordRequestContract
    {
        /// <summary>
        ///  Reset email
        /// </summary>
        /// <example>{{email}}</example>
        public string Email { get; init; } = string.Empty;
    }
}