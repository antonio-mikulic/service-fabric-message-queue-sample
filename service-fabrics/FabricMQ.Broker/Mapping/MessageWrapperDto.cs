namespace FabricMQ.Broker.Mapping
{
    public class MessageWrapperDto
    {
        public string Message { get; set; }
        public dynamic Payload { get; set; }
    }
}
