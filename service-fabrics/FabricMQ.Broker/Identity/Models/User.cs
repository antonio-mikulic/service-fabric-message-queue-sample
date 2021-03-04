using System;
using System.ComponentModel.DataAnnotations;
using FabricMQ.Broker.Database;

namespace FabricMQ.Broker.Identity.Models
{
    public class User : IEntity
    {
        [Key]
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }
        public virtual bool EmailConfirmed { get; set; }
        public virtual String PasswordHash { get; set; }
        public string NormalizedUserName { get; set; }
        public virtual string NormalizedEmailAddress { get; set; }
        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }

        public virtual void SetNormalizedNames()
        {
            NormalizedUserName = UserName.ToUpperInvariant();
            NormalizedEmailAddress = Email.ToUpperInvariant();
        }
    }
}
