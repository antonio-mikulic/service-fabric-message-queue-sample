using System;
using System.Collections.Generic;

namespace FabricMQ.Broker.Identity.Models
{
    public class CreateOrUpdateUserDto
    {
        public CreateOrUpdateUserDto()
        {
            Id = Guid.NewGuid();
            RoleNames = new List<string>();
        }

        public virtual Guid Id { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }
        public string Name { get; set; }
        public List<string> RoleNames { get; set; }
        public virtual string Password { get; set; }

    }
}
