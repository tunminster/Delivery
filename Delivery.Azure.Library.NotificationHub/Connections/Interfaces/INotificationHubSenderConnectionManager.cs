using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;

namespace Delivery.Azure.Library.NotificationHub.Connections.Interfaces
{
    public interface INotificationHubSenderConnectionManager : IConnectionManager<NotificationHubSenderConnection>
    {
    }
}