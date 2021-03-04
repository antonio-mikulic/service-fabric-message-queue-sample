using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace FabricMQ.Broker.Database
{
    public class ReliableEntityTable<T> : ReliableKeyedTable<Guid, T> , IReliableEntityTable<T> where T : class, IEntity<Guid>
    {
        public ReliableEntityTable(IReliableStateManager stateManager, IReliableTableTimeoutConfiguration timeoutConfiguration) : base(stateManager, timeoutConfiguration)
        {
        }

        public Task<T> Add(T item, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return Add(item.Id, item, timeout, cancellationToken);
        }

        public Task AddRange(IEnumerable<T> items, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return base.AddRange(items.ToDictionary(p => p.Id, p => p), timeout, cancellationToken);
        }

        public Task<T> Update(T item, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return Update(item.Id, item, timeout, cancellationToken);
        }

        public Task<T> Get(T item, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return base.Get(item.Id, timeout, cancellationToken);
        }

        public Task<bool> Remove(T item, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return base.Remove(item.Id, timeout, cancellationToken);
        }

        public Task RemoveRange(IEnumerable<T> items, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return base.RemoveRange(items.Select(s => s.Id), timeout, cancellationToken);
        }

        public Task<bool> Contains(T item, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return base.Contains(item.Id, timeout, cancellationToken);
        }
    }
}
