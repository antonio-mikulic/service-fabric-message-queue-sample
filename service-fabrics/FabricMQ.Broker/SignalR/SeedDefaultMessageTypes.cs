using System;
using System.Threading;
using System.Threading.Tasks;
using FabricMQ.Broker.Database;
using FabricMQ.Broker.Helpers;
using FabricMQ.Broker.Identity.Models;
using FabricMQ.Broker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FabricMQ.Broker.SignalR
{
    public class SeedDefaultMessageTypes
    {
        private readonly IUserStore<User> _userStore;
        private readonly IDatabaseContext _databaseContext;

        public SeedDefaultMessageTypes(IUserStore<User> userStore, IDatabaseContext context, ILogger<SeedDefaultMessageTypes> logger)
        {
            _userStore = userStore;
            this._databaseContext = context;
            Logger = logger;
        }

        public ILogger<SeedDefaultMessageTypes> Logger { get; }

        public async Task SeedAsync()
        {
            var cts = new CancellationTokenSource();


            var user = await _userStore.FindByNameAsync("admin", cts.Token);
            if (user != null)
            {
                // Get messageType count
                var messageTypes = await _databaseContext.MessageTypes.GetAll();

                if(messageTypes == null || messageTypes.Count == 0)
                {
                    Logger.LogInformation("No message types found in database, seed starting...");

                    var roleMessageType = new MessageType { Name = StaticSubscriptionNames.UserRoleNotificationString, Id = Guid.NewGuid() };
                    var userMessageType = new MessageType { Name = StaticSubscriptionNames.UserManagementNotificationString, Id = Guid.NewGuid() };

                    roleMessageType = await _databaseContext.MessageTypes.Add(roleMessageType);
                    userMessageType = await _databaseContext.MessageTypes.Add(userMessageType);

                    var roleAdminMapping = new MessageTypeUserMapping { UserId = user.Id, MessageTypeId = roleMessageType.Id, Id = Guid.NewGuid() };
                    var userAdminMapping = new MessageTypeUserMapping { UserId = user.Id, MessageTypeId = userMessageType.Id, Id = Guid.NewGuid() };

                    // For not loged in users
                    var defaultRoleMapping = new MessageTypeUserMapping { UserId = Guid.Empty, MessageTypeId = roleMessageType.Id, Id = Guid.NewGuid() };
                    var defaultUserMapping = new MessageTypeUserMapping { UserId = Guid.Empty, MessageTypeId = userMessageType.Id, Id = Guid.NewGuid() };


                    roleAdminMapping = await _databaseContext.MessageTypeUserMappings.Add(roleAdminMapping);
                    userAdminMapping = await _databaseContext.MessageTypeUserMappings.Add(userAdminMapping);

                    defaultRoleMapping = await _databaseContext.MessageTypeUserMappings.Add(defaultRoleMapping);
                    defaultUserMapping = await _databaseContext.MessageTypeUserMappings.Add(defaultUserMapping);


                    Logger.LogInformation("Default maps should be created!");

                }
            }
        }

    }
}
