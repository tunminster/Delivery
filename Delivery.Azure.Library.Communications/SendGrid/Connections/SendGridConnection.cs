using SendGrid;
using Delivery.Azure.Library.Connection.Managers;
using Delivery.Azure.Library.Connection.Managers.Interfaces;

namespace Delivery.Azure.Library.Communications.SendGrid.Connections
{
    public class SendGridConnection : Connection.Managers.Connection
    {
        public SendGridConnection(IConnectionMetadata connectionMetadata, SendGridClient sendGridClient) : base(connectionMetadata)
        {
            SendGridClient = sendGridClient;
        }
        
        public SendGridClient SendGridClient { get; }
    }
}