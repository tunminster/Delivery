using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.User.Domain.Contracts.Apple
{
    [DataContract]
    public class AppleLoginRequestContract
    {
        [DataMember]
        public string AuthorizationCode { get; set; }
        
        [DataMember]
        public string Email { get; set; }
        
        [DataMember]
        public string FamilyName { get; set; }
        
        [DataMember]
        public string GivenName { get; set; }
        
        [DataMember]
        public string MiddleName { get; set; }
        
        [DataMember]
        public string IdentityToken { get; set; }
        
        [DataMember]
        public string Nonce { get; set; }
        
        [DataMember]
        public bool RealUserStatus { get; set; }
        
        [DataMember]
        public string State { get; set; }
        
        [DataMember]
        public string User { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(AuthorizationCode)}: {AuthorizationCode.Format()}," +
                   $"{nameof(Email)}: {Email.Format()}," +
                   $"{nameof(FamilyName)}: {FamilyName.Format()}," +
                   $"{nameof(GivenName)}: {GivenName.Format()}," +
                   $"{nameof(MiddleName)}: {MiddleName.Format()}," +
                   $"{nameof(IdentityToken)}: {IdentityToken.Format()}," +
                   $"{nameof(Nonce)}: {Nonce.Format()}," +
                   $"{nameof(RealUserStatus)}: {RealUserStatus.Format()}," +
                   $"{nameof(State)}: {State.Format()}," +
                   $"{nameof(User)} : {User.Format()}";

        }
        
        
    }
}