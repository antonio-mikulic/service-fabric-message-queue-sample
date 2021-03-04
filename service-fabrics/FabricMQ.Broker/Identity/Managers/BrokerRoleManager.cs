using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FabricMQ.Broker.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FabricMQ.Broker.Identity.Managers
{
    public class BrokerRoleManager : RoleManager<Role>
    {
        public BrokerRoleManager(IRoleStore<Role> store, IEnumerable<IRoleValidator<Role>> roleValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<Role>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
        {
        }

        public override async Task<IdentityResult> CreateAsync(Role role)
        {
            role.SetNormalizedName();

            var result = await CheckDuplicateRoleNameAsync(null, role.Name);

            if (!result.Succeeded)
            {
                return result;
            }

            return await base.CreateAsync(role);
        }

        public override async Task<IdentityResult> UpdateAsync(Role role)
        {
            role.SetNormalizedName();

            var result = await CheckDuplicateRoleNameAsync(role.Id, role.Name);
            if (!result.Succeeded)
            {
                return result;
            }

            return await base.UpdateAsync(role);
        }


        public virtual async Task<IdentityResult> CheckDuplicateRoleNameAsync(Guid? expectedRoleId, string name)
        {
            var role = await FindByNameAsync(name);
            if (role != null && role.Id != expectedRoleId)
            {
                return IdentityResult.Failed(new IdentityError(){Description = $"Role with name {name} is already taken"});
            }

            return IdentityResult.Success;
        }
    }
}
