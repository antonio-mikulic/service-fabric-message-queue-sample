using System;

namespace FabricMQ.Broker.Database
{
    public interface IReliableTableTimeoutConfiguration
    {
        TimeSpan DefaultTimeout { get; set; }

        TimeSpan GetStateTimeout { get; set; }

        TimeSpan AddOrUpdateTimeout { get; set; }

        TimeSpan RemoveTimeout { get; set; }

        TimeSpan GetTimeout { get; set; }

        TimeSpan GetAllTimeout { get; set; }

        TimeSpan ClearTimeout { get; set; }
    }
}
