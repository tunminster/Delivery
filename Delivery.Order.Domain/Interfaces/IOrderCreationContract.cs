#nullable enable
namespace Delivery.Order.Domain.Interfaces
{
    public interface IOrderCreationContract
    {
        string? CardHolderName { get; set; }
    }
}