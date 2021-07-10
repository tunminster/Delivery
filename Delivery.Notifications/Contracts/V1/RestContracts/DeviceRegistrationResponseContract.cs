namespace Delivery.Notifications.Contracts.V1.RestContracts
{
    public record DeviceRegistrationResponseContract
    {
        /// <summary>
        ///  Device id
        /// </summary>
        public string Id { get; init; } = string.Empty;
    }
}