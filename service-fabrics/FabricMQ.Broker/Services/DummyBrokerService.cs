using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FabricMQ.Broker.Database;
using FabricMQ.Broker.Interfaces.Controllers;
using FabricMQ.Broker.Models;
using FabricMQ.Broker.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FabricMQ.Broker.Controllers
{
    public class DummyBrokerService : IBrokerService
    {
        private readonly NotificationsHub _notificationContext;

        private readonly IDatabaseContext _databaseContext;
        private IMapper ObjectMapper { get; set; }
        public ILogger<DummyBrokerService> Logger { get; }

        public DummyBrokerService(IMapper mapper, NotificationsHub notificationContext, IDatabaseContext context, ILogger<DummyBrokerService> logger)
        {
            ObjectMapper = mapper;
            _notificationContext = notificationContext;
            _databaseContext = context;
            Logger = logger;
        }

        public async Task<string> PublishMessage(string messageTypeKey, string message)
        {
            var clients = await FindSubscribedUserIdsForMessageType(messageTypeKey);
            await clients.SendAsync(messageTypeKey, message);
            return message;
        }

        public async Task<bool> IsUserSubscribedToMessageType(string userId, string messageTypeKey)
        {
            // Try to get a message type with same name
            var messageTypes = await _databaseContext.MessageTypes.GetAll();
            if (messageTypes.Count == 0)
            {
                Logger.LogInformation("No message types exist in database");
                return false;
            }

            var messageType = messageTypes.FirstOrDefault(s => s.Name == messageTypeKey);

            if (messageType == null)
            {
                Logger.LogInformation($"Message type with name {messageTypeKey} does not exist in the database!");
                return false;
            }

            var mappings = await _databaseContext.MessageTypeUserMappings.GetAll();
            if(mappings.Count == 0)
            {
                Logger.LogInformation("No mappings found in the database!");
                return false;
            }

            //if (userId.Length == 0 || userId == Guid.Empty.ToString())
            //{
            //    Logger.LogInformation("If userId isn't requested that means that all message types are shown by default");
            //    return true;
            //}

            var found = mappings.Any(s => s.MessageTypeId == messageType.Id && s.UserId.ToString() == userId);
            if (!found)
            {
                Logger.LogInformation($"User with id {userId} is not subscribed to the {messageTypeKey} publisher!");
                return false;
            }
            
            // All tests passed
            Logger.LogInformation($"User with id {userId} is subscribed to the {messageTypeKey} publisher!");
            return true;
        }

        public async Task<bool> SubscribeToMessageType(string userId, string messageTypeKey)
        {
            try
            {
                if (await IsUserSubscribedToMessageType(userId, messageTypeKey))
                {
                    return true;
                }

                var messageTypes = await _databaseContext.MessageTypes.GetAll();
                if (messageTypes.Count == 0)
                {
                    Logger.LogInformation("No message types exist in database");
                    return false;
                }

                var messageType = messageTypes.FirstOrDefault(s => s.Name == messageTypeKey);

                if (messageType == null)
                {
                    Logger.LogInformation($"Message type with name {messageTypeKey} does not exist in the database!");
                    return false;
                }

                var subscribed = await _databaseContext.MessageTypeUserMappings.Add(new MessageTypeUserMapping { UserId = Guid.Parse(userId), MessageTypeId = messageType.Id, Id = Guid.NewGuid() });

                // THIS SHOULD ADD CONNECTION ID NOT USER ID!
                await _notificationContext.Groups.AddToGroupAsync(_notificationContext.Context.ConnectionId, messageTypeKey);
                return true;
            }
            catch (System.Exception)
            {
                Logger.LogInformation($"Could not add user {userId} to subscription {messageTypeKey}");
                return false;
            }

        }

        public async Task<bool> UnsubscribeFromMessageType(string userId, string messageTypeKey)
        {
            if (!(await IsUserSubscribedToMessageType(userId, messageTypeKey)))
            {
                return true;
            }

            try
            {
                var messageTypes = await _databaseContext.MessageTypes.GetAll();
                var filteredMessageTypes = messageTypes.Where(s => s.Name == messageTypeKey).Select(s => s.Id);

                // TODO MULTIPLE MESSAGE TYPES MIGHT APPEAR, DUE TO SAME NAME FOR MULTIPLE USERS
                var mappings = await _databaseContext.MessageTypeUserMappings.GetAll();
                var map = mappings.FirstOrDefault(s => s.UserId.ToString() == userId && filteredMessageTypes.Contains(s.MessageTypeId));

                await _databaseContext.MessageTypeUserMappings.Remove(map);

                // THIS SHOULD REMOVE CONNECTION ID NOT USER ID!
                await _notificationContext.Groups.RemoveFromGroupAsync(_notificationContext.Context.ConnectionId, messageTypeKey);
                return true;
            }
            catch (Exception)
            {
                Logger.LogInformation($"Could not remove user {userId} to from subscription {messageTypeKey}");
                return false;
            }

        }

        public async Task<IClientProxy> FindSubscribedUserIdsForMessageType(string messageTypeId)
        {
            var clients = _notificationContext.Clients.Group(messageTypeId);
            await Task.Yield();
            return clients;
        }
    }
}
