namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record EmailContract
    {
        /// <summary>
        ///  The version of contract
        /// </summary>
        public int Version => 1;

        /// <summary>
        ///  Email address
        /// </summary>
        public string EmailAddress { get; init; } = string.Empty;

        /// <summary>
        ///  Name
        /// </summary>
        public string Name { get; init; } = string.Empty;
    }
}