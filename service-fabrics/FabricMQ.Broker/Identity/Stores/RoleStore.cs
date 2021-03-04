using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FabricMQ.Broker.Database;
using FabricMQ.Broker.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace FabricMQ.Broker.Identity.Stores
{
    /// <summary>
    /// This store isn't implemented as part of this sample.
    /// </summary>
    /// 
    // Creating instance of https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.rolestore-1?view=aspnetcore-2.2
    public class RoleStore : IRoleStore<Role>
    {
        public IDatabaseContext Context { get; }

        public RoleStore(IDatabaseContext context)
        {
            Context = context;
        }

        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (role == null) throw new ArgumentNullException(nameof(role));

            var roles = (await Context.Roles.GetAll(cancellationToken))?.ToList();

            var duplicated = roles?.FirstOrDefault(s => s.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase));

            if (duplicated != null)
                return IdentityResult.Failed(new IdentityError { Description = $"Role with name {role.Name} exists." });

            var created = await Context.Roles.Add(role.Id, role, cancellationToken:cancellationToken);

            return created != null ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Description = $"Could not insert role {role.Name}." });
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            var deleted = await Context.Roles.Remove(role.Id, cancellationToken: cancellationToken);

            return deleted ? IdentityResult.Success : IdentityResult.Failed(new IdentityError {Description = $"Could not delete role {role.Name}."});
        }

        public void Dispose()
        {
        }

        public async Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            if (Guid.TryParse(roleId, out Guid roleKey))
                return await Context.Roles.Get(roleKey, cancellationToken: cancellationToken);
            return null;
        }

        public async Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var roles = (await Context.Roles.GetAll(cancellationToken))?.ToList();

            return roles?.FirstOrDefault(s => s.NormalizedName.Equals(normalizedRoleName, StringComparison.OrdinalIgnoreCase));
        }

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public async Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            var foundRole = await Context.Roles.Get(role.Id, cancellationToken: cancellationToken);
            if (foundRole != null)
            {
                foundRole.NormalizedName = normalizedName;
                await Context.Roles.Update(foundRole.Id, foundRole, cancellationToken: cancellationToken);
            }
        }

        public async Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            var foundRole = await Context.Roles.Get(role.Id, cancellationToken: cancellationToken);
            if (foundRole != null)
            {
                foundRole.Name = roleName;
                await Context.Roles.Update(foundRole.Id, foundRole, cancellationToken: cancellationToken);
            }
        }

        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            var foundRole = await Context.Roles.Get(role.Id, cancellationToken: cancellationToken);
            if (foundRole != null)
            {
                var updated = await Context.Roles.Update(foundRole.Id, role, cancellationToken: cancellationToken);
                return updated != null ? IdentityResult.Success : IdentityResult.Failed();
            }

            return IdentityResult.Failed();
        }

        protected virtual string NormalizeKey(string key)
        {
            return key.ToUpperInvariant();
        }
    }
}