namespace Delivery.Managements.Domain.Contracts.V1.RestContracts.UpdateProfile
{
    /// <summary>
    ///  Update profile creation contract
    /// </summary>
    public record UpdateProfileCreationContract
    {
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