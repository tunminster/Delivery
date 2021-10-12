using System;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Customer.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Customer update status contract
    /// </summary>
    public record CustomerUpdateStatusContract
    {
        public int CustomerId { get; set; }

        public string Message { get; set; } = string.Empty;
        
        public DateTimeOffset UpdateDate { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(CustomerId)}: {CustomerId.Format()}," +
                   $"{nameof(Message)}: {Message.Format()}," +
                   $"{nameof(UpdateDate)}: {UpdateDate.Format()};";

        }
    }
}