using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FabricMQ.Broker.Helpers;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace FabricMQ.Broker.Database
{
    // Big thanks to Danijel Perić for help with this using State Manager as Database! 

    public class ReliableKeyedTable<TKey, TEntity> : IReliableKeyedTable<TKey, TEntity> where TEntity : class where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        public IReliableStateManager StateManager { get; }
        public IReliableTableTimeoutConfiguration TimeoutConfiguration { get; }

        public ReliableKeyedTable(IReliableStateManager stateManager, IReliableTableTimeoutConfiguration timeoutConfiguration)
        {
            StateManager = stateManager;
            TimeoutConfiguration = timeoutConfiguration;
        }

        public async Task<TEntity> Add(TKey key, TEntity item, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            var data = await GetState(TimeoutConfiguration.GetStateTimeout, cancellationToken);
           
            return await TimeoutRetryHelper.ExecuteInTransaction(StateManager, async (tx, token, state) =>
            {
                if (await data.ContainsKeyAsync(tx, key, GetTimeout(timeout, TimeoutConfiguration.GetTimeout), cancellationToken))
                {
                    throw new Exception("Duplicate id " + key);
                }

                //EntityHelper.UpdateKey(key, item);

                var added = await data.TryAddAsync(tx, key, item, GetTimeout(timeout, TimeoutConfiguration.AddOrUpdateTimeout), token);

                return added ? item : null;
            }, cancellationToken: cancellationToken);
        }

        public async Task AddRange(IReadOnlyDictionary<TKey, TEntity> items, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            foreach (var pair in items)
            {
                await Add(pair.Key, pair.Value, timeout, cancellationToken);
            }
        }

        public async Task<TEntity> Update(TKey key, TEntity item, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            var data = await GetState(TimeoutConfiguration.GetStateTimeout, cancellationToken);
           
            return await TimeoutRetryHelper.ExecuteInTransaction(StateManager, async (tx, token, state) =>
            {
                var found = await data.TryGetValueAsync(tx, key, LockMode.Update, TimeoutConfiguration.GetTimeout, cancellationToken);
                if (found.HasValue)
                {
                    bool updated = await data.TryUpdateAsync(tx, key, item, found.Value, GetTimeout(timeout, TimeoutConfiguration.AddOrUpdateTimeout), token);
                    
                    if (updated)
                        return item;
                }
                return null;
            }, cancellationToken: cancellationToken);
        }

        public async Task<bool> Remove(TKey key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            var data = await GetState(TimeoutConfiguration.GetStateTimeout, cancellationToken);
           
            return await TimeoutRetryHelper.ExecuteInTransaction(StateManager, async (tx, token, state) =>
            {
                var result = await data.TryRemoveAsync(tx, key, GetTimeout(timeout, TimeoutConfiguration.RemoveTimeout), token);
                return result.HasValue;

            }, cancellationToken: cancellationToken);
        }

        public async Task RemoveRange(IEnumerable<TKey> keys, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            var data = await GetState(TimeoutConfiguration.GetStateTimeout, cancellationToken);
           
            await TimeoutRetryHelper.ExecuteInTransaction(StateManager, async (tx, token, state) =>
            {
                foreach (var key in keys)
                   await data.TryRemoveAsync(tx, key, GetTimeout(timeout, TimeoutConfiguration.RemoveTimeout), token);
            }, cancellationToken: cancellationToken);
        }

        public async Task Clear(TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            var data = await GetState(TimeoutConfiguration.GetStateTimeout, cancellationToken);

            await TimeoutRetryHelper.ExecuteInTransaction(StateManager, async (tx, token, state) =>
            {
                await data.ClearAsync(GetTimeout(timeout, TimeoutConfiguration.ClearTimeout), token);

            }, cancellationToken: cancellationToken);
        }

        public async Task<bool> Contains(TKey key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            var data = await GetState(TimeoutConfiguration.GetStateTimeout, cancellationToken);

            return await TimeoutRetryHelper.ExecuteInTransaction(StateManager, async (tx, token, state) => await data.ContainsKeyAsync(tx, key, GetTimeout(timeout, TimeoutConfiguration.GetTimeout), token), cancellationToken: cancellationToken);
        }

        public async Task<TEntity> Get(TKey key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            var data = await GetState(TimeoutConfiguration.GetStateTimeout, cancellationToken);

            return await TimeoutRetryHelper.ExecuteInTransaction(StateManager, async (tx, token, state) =>
            {
                var found = await data.TryGetValueAsync(tx, key, GetTimeout(timeout, TimeoutConfiguration.GetTimeout), token);

                return found.Value;

            }, cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<TEntity>> GetAll(CancellationToken cancellationToken = default)
        {
            var data = await GetState(TimeoutConfiguration.GetStateTimeout, cancellationToken);

            List<TEntity> result = new List<TEntity>();

            await TimeoutRetryHelper.ExecuteInTransaction(StateManager, async (tx, token, state) =>
            {
                var r = (List<TEntity>)state;

                var enumerable = await data.CreateEnumerableAsync(tx, EnumerationMode.Unordered);

                using (var e = enumerable.GetAsyncEnumerator())
                    while (await e.MoveNextAsync(token).ConfigureAwait(false))
                        r.Add(e.Current.Value);

            }, result, cancellationToken);

            return result;
        }

        private async Task<IReliableDictionary<TKey, TEntity>> GetState(TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
           return await TimeoutRetryHelper.Execute((token, state) => StateManager.GetOrAddAsync<IReliableDictionary<TKey, TEntity>>(typeof(TEntity).Name, GetTimeout(timeout, TimeoutConfiguration.GetStateTimeout)), cancellationToken: cancellationToken);
        }

        public TimeSpan GetTimeout(TimeSpan requestedTimeout, TimeSpan defaultTimeout)
        {
            return requestedTimeout != default ? requestedTimeout : defaultTimeout;
        }
    }
}
