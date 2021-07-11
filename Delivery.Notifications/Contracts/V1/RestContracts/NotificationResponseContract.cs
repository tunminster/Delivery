using System;
using Delivery.Notifications.Contracts.V1.Enums;

namespace Delivery.Notifications.Contracts.V1.RestContracts
{
    public record NotificationResponseContract
    {
        public NotificationStatus Status { get; init; }
        
        public DateTimeOffset DateCreated { get; init; }
    }
}