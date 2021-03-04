using System;

namespace FabricMQ.Broker.Identity.Models
{
    /// <summary>
    /// Represents role record of a user. 
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// User id.
        /// </summary>
        public virtual Guid UserId { get; set; }

        /// <summary>
        /// Role id.
        /// </summary>
        public virtual Guid RoleId { get; set; }

        /// <summary>
        /// Creates a new <see cref="UserRole"/> object.
        /// </summary>
        public UserRole()
        {

        }

        /// <summary>
        /// Creates a new <see cref="UserRole"/> object.
        /// </summary>
        /// <param name="tenantId">Tenant id</param>
        /// <param name="userId">User id</param>
        /// <param name="roleId">Role id</param>
        public UserRole(Guid userId, Guid roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}