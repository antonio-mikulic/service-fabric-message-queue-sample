using System;
using FabricMQ.Broker.Identity.Models;
using FabricMQ.Broker.Models;
using Microsoft.ServiceFabric.Data;

namespace FabricMQ.Broker.Database
{
    public class BrokerDatabaseContext : IDatabaseContext
    {
        public BrokerDatabaseContext(IReliableStateManager stateManager, IReliableTableTimeoutConfiguration timeoutProvider)
        {
            Users = new ReliableEntityTable<User>(stateManager, timeoutProvider);
            Roles = new ReliableEntityTable<Role>(stateManager, timeoutProvider);
            Claims = new ReliableKeyedTable<Guid, UserClaims>(stateManager, timeoutProvider);
            UserRoles = new ReliableKeyedTable<Guid, UserRoles>(stateManager, timeoutProvider);

            MessageTypes = new ReliableEntityTable<MessageType>(stateManager, timeoutProvider);
            MessageTypeUserMappings = new ReliableEntityTable<MessageTypeUserMapping>(stateManager, timeoutProvider);
        }

        public IReliableEntityTable<User> Users { get; }
        public IReliableEntityTable<Role> Roles { get; }
        public IReliableKeyedTable<Guid, UserClaims> Claims { get; }
        public IReliableKeyedTable<Guid, UserRoles> UserRoles { get; }

        public IReliableEntityTable<MessageType> MessageTypes { get; }
        public IReliableEntityTable<MessageTypeUserMapping> MessageTypeUserMappings { get; }
    }
}
