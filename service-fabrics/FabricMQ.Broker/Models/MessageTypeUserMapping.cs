using FabricMQ.Broker.Database;

namespace FabricMQ.Broker.Models
{
    public class MessageTypeUserMapping : IEntity
    {
        public System.Guid Id { get; set; }
        public System.Guid MessageTypeId { get; set; }
        public System.Guid UserId { get; set; }
    }
}