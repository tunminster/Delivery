#nullable enable
namespace Delivery.Order.Domain.Contracts.Interfaces
{
    public interface IOrderCreationContract
    {
        string? CardHolderName { get; set; }
    }
}