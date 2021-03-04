using System;
using FabricMQ.Broker.Identity.Models;
using FabricMQ.Broker.Models;

namespace FabricMQ.Broker.Database
{
    public interface IDatabaseContext
    {
        IReliableEntityTable<User> Users { get; }
        IReliableEntityTable<Role> Roles { get; }
        IReliableEntityTable<MessageType> MessageTypes { get; }
        IReliableEntityTable<MessageTypeUserMapping> MessageTypeUserMappings { get; }
        IReliableKeyedTable<Guid, UserClaims> Claims { get; }
        IReliableKeyedTable<Guid, UserRoles> UserRoles { get; }
    }
}