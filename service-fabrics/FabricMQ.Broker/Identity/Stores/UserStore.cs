using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FabricMQ.Broker.Database;
using FabricMQ.Broker.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace FabricMQ.Broker.Identity.Stores
{
    /// <summary>
    /// This store is only partially implemented. It supports user creation and find methods.
    /// </summary>
    /// Creating instance of https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.userstore-1?view=aspnetcore-2.2
    public class UserStore : IUserPasswordStore<User>, IUserClaimStore<User>, IUserRoleStore<User>, IUserStore<User>, IUserEmailStore<User>
    {
        public IDatabaseContext Context { get; }

        public UserStore(IDatabaseContext context)
        {
            Context = context;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));

            var created = await Context.Users.Add(user.Id, user, cancellationToken:cancellationToken);

            return created != null ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Description = $"Could not insert user {user.Email}." });
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
         
            if (user == null) throw new ArgumentNullException(nameof(user));

            var removed = await Context.Users.Remove(user.Id, cancellationToken:cancellationToken);

            return removed ? IdentityResult.Success : IdentityResult.Failed(new IdentityError {Description = $"Could not delete user {user.Email}."});
        }

        public void Dispose()
        {
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (userId == null) throw new ArgumentNullException(nameof(userId));
           
            if(!Guid.TryParse(userId, out var idGuid))
                throw new ArgumentException("Not a valid Guid id", nameof(userId));
            
            return await Context.Users.Get(idGuid, cancellationToken:cancellationToken);
        }

        public async Task<User> FindByNameAsync(string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (userName == null) throw new ArgumentNullException(nameof(userName));

            var normalizedName = NormalizeKey(userName);

            var users = await Context.Users.GetAll(cancellationToken);

            var found = users.FirstOrDefault(s => s.NormalizedUserName == normalizedName);

            return found;
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.UserName);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (normalizedName == null) throw new ArgumentNullException(nameof(normalizedName));

            user.NormalizedUserName = normalizedName;
            return Task.FromResult<object>(null);
        }

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (passwordHash == null) throw new ArgumentNullException(nameof(passwordHash));

            user.PasswordHash = passwordHash;
            return Task.FromResult<object>(null);

        }

        public async Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            var foundUser = await Context.Users.Get(user.Id, cancellationToken:cancellationToken);
            foundUser.UserName = userName;
            await Context.Users.Update(foundUser.Id, foundUser, cancellationToken: cancellationToken);
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(user.NormalizedUserName))
                user.NormalizedUserName = NormalizeKey(user.UserName);

            var updated = await Context.Users.Update(user.Id, user, cancellationToken: cancellationToken);

           return updated != null ? IdentityResult.Success : IdentityResult.Failed();
        }

        public async Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            var userClaims = await Context.Claims.Get(user.Id, cancellationToken: cancellationToken);

            return userClaims != null ? userClaims.Claims : (IList<Claim>)new Claim[] { };
        }

        public async Task AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            var userClaims = await Context.Claims.Get(user.Id, cancellationToken: cancellationToken);

            if (userClaims != null)
            {
                foreach (var cl in claims)
                    userClaims.Claims.Add(cl);
                await Context.Claims.Update(user.Id, userClaims, cancellationToken: cancellationToken);
            }
            else
            {
                userClaims = new UserClaims {UserId = user.Id};
                foreach (var cl in claims)
                    userClaims.Claims.Add(cl);
                await Context.Claims.Add(user.Id, userClaims, cancellationToken:cancellationToken);
            }
        }

        public async Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            var userClaims = await Context.Claims.Get(user.Id, cancellationToken: cancellationToken);
            if (userClaims != null)
            {
                if (userClaims.Claims.Contains(claim))
                    userClaims.Claims.Remove(claim);
                userClaims.Claims.Add(newClaim);
            }
            else
            {
                userClaims = new UserClaims {UserId = user.Id};
                userClaims.Claims.Add(newClaim);
            }

            await Context.Claims.Update(user.Id, userClaims, cancellationToken: cancellationToken);

        }

        public async Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            var userClaims = await Context.Claims.Get(user.Id, cancellationToken: cancellationToken);
            if (userClaims != null)
            {
                foreach (var claim in claims)
                    userClaims.Claims.Remove(claim);

                await Context.Claims.Update(user.Id, userClaims, cancellationToken: cancellationToken);
            }
        }

        public async Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
           List<User> users = new List<User>();

           var claims = await Context.Claims.GetAll(cancellationToken);

           foreach (var userClaim in claims)
           {
               if (userClaim.Claims.Contains(claim))
               {
                   var user = await Context.Users.Get(userClaim.UserId, cancellationToken: cancellationToken);
                   if (user != null)
                   {
                       users.Add(user);
                   }
               }
           }

           return users;
        }

        protected virtual string NormalizeKey(string key)
        {
            return key.ToUpperInvariant();
        }

        public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var role = await GetRoleByNameAsync(roleName);

            if (role != null)
            {
                var userRoles = await Context.UserRoles.Get(user.Id, cancellationToken: cancellationToken);

                if (userRoles == null)
                {
                    userRoles = new UserRoles();
                    userRoles.Roles.Add(new UserRole(user.Id, role.Id));
                    await Context.UserRoles.Add(user.Id, userRoles, cancellationToken: cancellationToken);
                }
                else
                {
                    if (userRoles.Roles.All(s => s.RoleId != role.Id))
                    {
                        userRoles.Roles.Add(new UserRole(user.Id, role.Id));
                    }
                }
            }
        }

        public async Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var role = await GetRoleByNameAsync(roleName);

            if (role != null)
            {
                var userRoles = await Context.UserRoles.Get(user.Id, cancellationToken: cancellationToken);

                if (userRoles != null)
                {
                    userRoles.Roles.RemoveAll(s => s.RoleId == role.Id);

                    await Context.UserRoles.Update(user.Id, userRoles, cancellationToken: cancellationToken);
                }
            }
        }

        public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            List<string> result = new List<string>();

            var userRoles = await Context.UserRoles.Get(user.Id, cancellationToken: cancellationToken);
            var roles = await Context.Roles.GetAll(cancellationToken: cancellationToken);

            if (userRoles?.Roles == null) return result;

            foreach (var userRole in userRoles?.Roles)
            {
                var role = roles.FirstOrDefault(s => s.Id == userRole.RoleId);
                if (role != null)
                {
                    result.Add(role.Name);
                }
            }

            return result;
        }

        public async Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var role = await GetRoleByNameAsync(roleName);

            if (role != null)
            {
                var userRoles = await Context.UserRoles.Get(user.Id, cancellationToken: cancellationToken);

                return userRoles != null && userRoles.Roles.Any(s => s.RoleId == role.Id);
            }

            return false;
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            Dictionary<Guid, User> result = new Dictionary<Guid, User>();

            var role = await GetRoleByNameAsync(roleName);

            if (role != null)
            {
                var userRoles = await Context.UserRoles.GetAll(cancellationToken: cancellationToken);
                var users = await Context.Users.GetAll(cancellationToken: cancellationToken);

                IEnumerable<Guid> usersInRole = userRoles.SelectMany(s => s.Roles).Where(s => s.RoleId == role.Id).Select(s => s.UserId).Distinct();

                foreach (var id in usersInRole)
                {
                    if (!result.ContainsKey(id))
                    {
                        var user = users.FirstOrDefault(s => s.Id == id);
                        if (user != null)
                        {
                            result.Add(user.Id, user);
                        }
                    }
                }
            }

            return result.Values.ToList();
        }

        private async Task<Role> GetRoleByNameAsync(string roleName)
        {
            var normalizedName = NormalizeKey(roleName);
            var role = (await Context.Roles.GetAll()).FirstOrDefault(r => r.NormalizedName == normalizedName);
            if (role == null)
            {
                throw new Exception("Could not find a role with name: " + normalizedName);
            }

            return role;
        }

        public async Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            await Context.Users.Update(user, cancellationToken: cancellationToken);
        }

        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public async Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            await Context.Users.Update(user, cancellationToken: cancellationToken);
        }

        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var users = await Context.Users.GetAll(cancellationToken);

            return users.FirstOrDefault(s => s.NormalizedEmailAddress == normalizedEmail);
        }

        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmailAddress);
        }

        public async Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmailAddress = normalizedEmail;
            await Context.Users.Update(user, cancellationToken: cancellationToken);
        }
    }
}