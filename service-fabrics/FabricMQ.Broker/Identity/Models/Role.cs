using System;
using System.ComponentModel.DataAnnotations;
using FabricMQ.Broker.Database;

namespace FabricMQ.Broker.Identity.Models
{
    public class Role : IEntity
    {
        [Key]
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string NormalizedName { get; set; }

        public virtual void SetNormalizedName()
        {
            NormalizedName = Name.ToUpperInvariant();
        }
    }
}
