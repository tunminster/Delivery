using Delivery.Database.Enums;

namespace Delivery.Driver.Domain.Factories
{
    public record DriverEmailTemplateModel(BusinessDomainType BusinessDomainType, string TemplateId, string Subject, object? DynamicTemplateData, string NotificationUniqueId);
}

