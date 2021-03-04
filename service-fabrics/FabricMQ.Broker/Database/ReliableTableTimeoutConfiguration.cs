using System;

namespace FabricMQ.Broker.Database
{
    public class ReliableTableTimeoutConfiguration : IReliableTableTimeoutConfiguration
    {
        public ReliableTableTimeoutConfiguration()
        {
            DefaultTimeout = TimeSpan.FromSeconds(30);
            GetStateTimeout = TimeSpan.FromSeconds(10);
            AddOrUpdateTimeout = TimeSpan.FromSeconds(10);
            RemoveTimeout = TimeSpan.FromSeconds(10);
            GetTimeout = TimeSpan.FromSeconds(10);
            GetAllTimeout = TimeSpan.FromMinutes(1);
            ClearTimeout = TimeSpan.FromSeconds(30);
        }

        public TimeSpan DefaultTimeout { get; set; }
        public TimeSpan GetStateTimeout { get; set; }
        public TimeSpan AddOrUpdateTimeout { get; set; }
        public TimeSpan RemoveTimeout { get; set; }
        public TimeSpan GetTimeout { get; set; }
        public TimeSpan GetAllTimeout { get; set; }
        public TimeSpan ClearTimeout { get; set; }
    }
}
