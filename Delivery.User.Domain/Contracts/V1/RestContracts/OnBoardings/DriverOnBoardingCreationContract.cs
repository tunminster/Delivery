using Delivery.Database.Enums;

namespace Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings
{
    public record DriverOnBoardingCreationContract
    {
        public string FirstName { get; init; } = string.Empty;
        
        public string LastName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string JobTitle { get; init; } = string.Empty;

        public string DateOfBirth { get; init; } = string.Empty;

        public IdentityDocumentType IdentityDocumentType { get; init; }

        public string SocialSecurityNumber { get; init; } = string.Empty;

        public OnBoardingAddressContract Address { get; init; } = new();

        public OnBoardingBankAccountContract BankAccount { get; init; } = new();
    }
}