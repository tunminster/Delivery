using Delivery.Azure.Library.Communications.SendGrid.Connections;
using Delivery.Azure.Library.Connection.Managers.Interfaces;

namespace Delivery.Azure.Library.Communications.SendGrid.Interfaces
{
    /// <summary>
    ///  Private connection to sendgrid
    /// </summary>
    public interface ISendGridConnectionManager : IConnectionManager<SendGridConnection>
    {
        
    }
}