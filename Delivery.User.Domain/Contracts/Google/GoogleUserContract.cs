using System.Runtime.Serialization;

namespace Delivery.User.Domain.Contracts.Google
{
    [DataContract]
    public class GoogleUserContract
    {
        [DataMember]
        public string Iss { get; set; }
        
        [DataMember]
        public string Sub { get; set; }
        
        [DataMember]
        public string Email { get; set; }
        
        [DataMember]
        public bool EmailVerified { get; set; }
        
        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public string Picture { get; set; }
        
        [DataMember]
        public string GivenName { get; set; }
        
        [DataMember]
        public string FamilyName { get; set; }
        
        [DataMember]
        public string Locale { get; set; }
    }
}