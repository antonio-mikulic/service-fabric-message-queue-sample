using System;

namespace FabricMQ.Broker.Database
{
    public interface IEntity<TKey> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        TKey Id { get; set; }
    }

    public interface IEntity : IEntity<Guid>
    {
    }
}
