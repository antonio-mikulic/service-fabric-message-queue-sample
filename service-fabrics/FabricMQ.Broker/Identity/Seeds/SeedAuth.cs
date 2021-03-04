using System.Threading;
using System.Threading.Tasks;
using FabricMQ.Broker.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace FabricMQ.Broker.Identity.Seeds
{
    public class SeedAuth
    {
        private readonly IRoleStore<Role> _roleStore;
        private readonly IUserStore<User> _userStore;
        private readonly IUserRoleStore<User> _userRoleStore;
        private readonly IPasswordHasher<User> _hasher;

        public SeedAuth(IRoleStore<Role> roleStore, IUserStore<User> userStore, IUserRoleStore<User> userRoleStore, IPasswordHasher<User> hasher)
        {
            _roleStore = roleStore;
            _userStore = userStore;
            _userRoleStore = userRoleStore;
            _hasher = hasher;
        }

        public async Task SeedAsync()
        {
            var cts = new CancellationTokenSource();

            Role role = await _roleStore.FindByNameAsync("Admin", cts.Token);

            if (role == null)
            {
                role = new Role { Name = "Admin"};
                role.SetNormalizedName();
                await _roleStore.CreateAsync(role, cts.Token);
            }

            var user = await _userStore.FindByNameAsync("admin", cts.Token);
            if (user == null)
            {
                user = new User { UserName = "admin", Email = "admin@admin.com", EmailConfirmed = true};
                user.SetNormalizedNames();
                user.PasswordHash = _hasher.HashPassword(user, "123qwe");
                await _userStore.CreateAsync(user, cts.Token);
                await _userRoleStore.AddToRoleAsync(user, role.Name, cts.Token);
            }
        }
    }
}