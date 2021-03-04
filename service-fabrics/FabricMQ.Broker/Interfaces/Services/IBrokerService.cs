using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FabricMQ.Broker.Interfaces.Controllers
{
    public interface IBrokerService
    {
        Task<string> PublishMessage(string messageTypeId, string message);

        Task<bool> SubscribeToMessageType(string userId, string messageTypeId);
        Task<bool> UnsubscribeFromMessageType(string userId, string messageTypeId);

        Task<IClientProxy> FindSubscribedUserIdsForMessageType(string messageTypeId);

        Task<bool> IsUserSubscribedToMessageType(string userId, string messageTypeKey);
    }
}
