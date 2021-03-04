using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FabricMQ.Broker.Database
{
    public interface IReliableEntityTable<TEntity> : IReliableKeyedTable<Guid, TEntity> where TEntity : IEntity<Guid>
    {
        Task<TEntity> Add(TEntity item, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task AddRange(IEnumerable<TEntity> items, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task<TEntity> Update(TEntity item, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task<TEntity> Get(TEntity item, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task<bool> Remove(TEntity item, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task RemoveRange(IEnumerable<TEntity> items, TimeSpan timeout = default, CancellationToken cancellationToken = default);
        Task<bool> Contains(TEntity item, TimeSpan timeout = default, CancellationToken cancellationToken = default);
    }
}
