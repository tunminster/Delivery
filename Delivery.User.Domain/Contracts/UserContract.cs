using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.User.Domain.Contracts
{
    [DataContract]
    public class UserContract
    {
        [DataMember]
        public string UserName { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(UserName)} : {UserName.Format()}";

        }
    }
}