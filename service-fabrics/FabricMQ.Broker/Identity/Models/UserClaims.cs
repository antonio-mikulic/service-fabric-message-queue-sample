using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace FabricMQ.Broker.Identity.Models
{
    public class UserClaims
    {
        public UserClaims()
        {
            Claims = new List<Claim>();
        }
        public Guid UserId { get; set; }
        public List<Claim> Claims { get; set; }
    }
}
