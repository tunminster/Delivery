namespace Delivery.Driver.Domain.Contracts.V1.Models;

/// <summary>
///  Driver approved model
/// </summary>
public record DriverApprovedDataModel
{
    /// <summary>
    ///  Subject
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    ///  Driver name
    /// </summary>
    public string Name { get; init; } = string.Empty;
}