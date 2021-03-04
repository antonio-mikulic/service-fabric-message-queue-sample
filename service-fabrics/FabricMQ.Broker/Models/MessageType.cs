using FabricMQ.Broker.Database;

namespace FabricMQ.Broker.Models
{
    public class MessageType : IEntity
    {
        public System.Guid Id { get; set; }
        public string Name { get; set; }
    }
}