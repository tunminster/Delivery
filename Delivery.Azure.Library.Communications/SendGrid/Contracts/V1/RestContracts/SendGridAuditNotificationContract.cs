namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record SendGridAuditNotificationContract(SendGridEmailCreationContract SendGridEmailCreationContract,
        SendGridEmailNotificationStatusContract SendGridEmailNotificationStatusContract);

}