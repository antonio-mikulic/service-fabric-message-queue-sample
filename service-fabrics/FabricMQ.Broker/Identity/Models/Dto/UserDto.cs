using System;
using System.Collections.Generic;

namespace FabricMQ.Broker.Identity.Models.Dto
{
    public class UserDto
    {
        public virtual Guid Id { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }
        public string NormalizedUserName { get; set; }
        public string Name { get; set; }
        public List<string> RoleNames { get; set; }
    }
}
