using FabricMQ.Broker.Mapping;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FabricMQ.Broker.Interfaces.Controllers
{
    public interface IBrokerController
    {
        Task<MessageWrapperDto> PublishMessage(string messageTypeId, string message);

        Task<SuccessResponseDto> SubscribeToMessageType(string messageTypeId);
        Task<SuccessResponseDto> UnsubscribeFromMessageType(string messageTypeId);

        Task<IClientProxy> FindSubscribedUserIdsForMessageType(string messageTypeKey);

        Task<SuccessResponseDto> IsUserSubscribedToMessageType(string messageTypeKey);
    }
}
