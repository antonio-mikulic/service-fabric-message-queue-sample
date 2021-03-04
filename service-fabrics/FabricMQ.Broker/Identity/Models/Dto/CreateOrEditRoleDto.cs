using System;
using System.Collections.Generic;

namespace FabricMQ.Broker.Identity.Models.Dto
{
    public class CreateOrEditRoleDto
    {
        public CreateOrEditRoleDto()
        {
            Id = Guid.NewGuid();
        }

        public virtual Guid Id { get; set; }
        public string Name { get; set; }
    }
}
