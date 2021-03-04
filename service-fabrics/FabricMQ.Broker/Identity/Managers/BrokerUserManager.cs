using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FabricMQ.Broker.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FabricMQ.Broker.Identity.Managers
{
    public class BrokerUserManager : UserManager<User>
    {
        public BrokerUserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override async Task<IdentityResult> CreateAsync(User user)
        {
            user.SetNormalizedNames();

            var result = await CheckDuplicateUsernameOrEmailAddressAsync(null, user.UserName, user.Email);

            if (!result.Succeeded)
            {
                return result;
            }

            return await base.CreateAsync(user);
        }

        public override async Task<IdentityResult> CreateAsync(User user, string password)
        {
            user.SetNormalizedNames();

            var result = await CheckDuplicateUsernameOrEmailAddressAsync(null, user.UserName, user.Email);

            if (!result.Succeeded)
            {
                return result;
            }

            return await base.CreateAsync(user, password);
        }

        public override async Task<IdentityResult> UpdateAsync(User user)
        {
            user.SetNormalizedNames();

            var result = await CheckDuplicateUsernameOrEmailAddressAsync(user.Id, user.UserName, user.Email);

            if (!result.Succeeded)
            {
                return result;
            }

            return await base.UpdateAsync(user);
        }

        public virtual async Task<IdentityResult> CheckDuplicateUsernameOrEmailAddressAsync(Guid? expectedUserId, string userName, string emailAddress)
        {
            var user = (await FindByNameAsync(userName));
            if (user != null && user.Id != expectedUserId)
            {
                return IdentityResult.Failed(new IdentityError {Description = $"Duplicate UserName {userName}"});
            }

            user = (await FindByEmailAsync(emailAddress));
            if (user != null && user.Id != expectedUserId)
            {
                return IdentityResult.Failed(new IdentityError {Description = $"Duplicate Email {emailAddress}"});
            }

            return IdentityResult.Success;
        }

    }
}
