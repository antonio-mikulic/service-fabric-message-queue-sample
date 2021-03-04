using System.Collections.Generic;

namespace FabricMQ.Broker.Identity.Models
{
    public class UserRoles
    {
        public UserRoles()
        {
            Roles = new List<UserRole>();
        }

        public List<UserRole> Roles { get; set; }
    }
}
