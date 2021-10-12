using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Customer.Domain.Contracts.V1.RestContracts
{
    public record CustomerUpdateContract
    {
        public int CustomerId { get; set; }

        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;

        public string ContactNumber { get; init; } = string.Empty;
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(CustomerId)}: {CustomerId.Format()}," +
                   $"{nameof(FirstName)}: {FirstName.Format()}," +
                   $"{nameof(LastName)}: {LastName.Format()}," +
                   $"{nameof(ContactNumber)}: {ContactNumber.Format()};";

        }
    }
}