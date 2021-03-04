namespace FabricMQ.Broker.Models
{
    public class ResponseMessage<T>
    {
        public T Result { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
