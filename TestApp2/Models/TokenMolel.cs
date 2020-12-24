using System;
using System.Runtime.Serialization;

namespace TestApp2.Models
{
    public class TokenMolel
    {
        [DataContract]
        public class TokenModel
        {
            [DataMember(Name = "access_token")]
            public String AccessToken { get; set; }

            [DataMember(Name = "token_type")]
            public String TokenType { get; set; }

            [DataMember(Name = "refresh_token")]
            public String RefreshToken { get; set; }

            [DataMember(Name = "expires_in")]
            public int ExpiresIn { get; set; }

            public bool IsPending { get; set; }
        }

    }
}