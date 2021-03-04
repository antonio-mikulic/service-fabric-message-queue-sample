using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FabricMQ.Broker.Database
{
    public interface IReliableKeyedTable<TKey, TEntity> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        Task<TEntity> Add(TKey key, TEntity item, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task AddRange(IReadOnlyDictionary<TKey, TEntity> items, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task<TEntity> Update(TKey key, TEntity item, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task<TEntity> Get(TKey key, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> GetAll(CancellationToken cancellationToken = default);
        Task<bool> Remove(TKey key, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task RemoveRange(IEnumerable<TKey> keys, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task Clear(TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task<bool> Contains(TKey key, TimeSpan timeout = default, CancellationToken cancellationToken = default);
    }
}