using System;

namespace FabricMQ.Broker.Models
{
    public class AuthenticateResultModel
    {
        public string AccessToken { get; set; }

        public string EncryptedAccessToken { get; set; }

        public int ExpireInSeconds { get; set; }

        public DateTime ExpireIn { get; set; }

        public Guid UserId { get; set; }
    }
}
