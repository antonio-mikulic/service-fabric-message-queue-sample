using FabricMQ.Broker.Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FabricMQ.Broker.SignalR
{
    public class NotificationsHub : Hub
    {
        private readonly IDatabaseContext _databaseContext;

        public ILogger<NotificationsHub> Logger { get; }

        public NotificationsHub(IDatabaseContext context, ILogger<NotificationsHub> logger)
        {
            _databaseContext = context;
            Logger = logger;
        }

        public Task HeartBeatTock()
        {
            return Task.CompletedTask;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var userId = Context.UserIdentifier;

            if (userId == null || userId.Length < 5)
                userId = Guid.Empty.ToString();

            Logger.LogInformation($"User connected with ConnectionId {Context.ConnectionId}. His UserIndentifier is {userId}!");

            // Get all user message type mappings from database
            var messageTypeMappings = await _databaseContext.MessageTypeUserMappings.GetAll();

            // Get user message type ids
            var filteredMappings = messageTypeMappings.Where(s => s.UserId.ToString() == userId);

            foreach (var id in filteredMappings)
            {
                // Find message type with id and if it exists add user to that group
                try
                {
                    var messageType = await _databaseContext.MessageTypes.Get(id.MessageTypeId);

                    await Groups.AddToGroupAsync(Context.ConnectionId, messageType.Name);
                    Logger.LogInformation($"Added user with connected from {Context.ConnectionId} to message group {messageType.Name}");
                }
                catch (System.Exception)
                {
                    // Log warning in case message type with this id isn't found
                    Logger.LogWarning($"Message type with id {id} not found for user {Context.ConnectionId}!");
                }
            }
        }
    }
}
