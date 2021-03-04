using FabricMQ.Broker.Identity.Models;
using FabricMQ.Broker.Interfaces.Controllers;
using FabricMQ.Broker.Mapping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FabricMQ.Broker.Controllers
{
    [Route("api/[controller]/[action]")]
    public class BrokerController : Controller, IBrokerController
    {
        private readonly IBrokerService _brokerService;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BrokerController(IBrokerService brokerService, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _brokerService = brokerService;
            this._userManager = userManager;
            this._httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task<IClientProxy> FindSubscribedUserIdsForMessageType(string messageTypeId)
        {
            return await _brokerService.FindSubscribedUserIdsForMessageType(messageTypeId);
        }

        [HttpPost]
        public async Task<SuccessResponseDto> IsUserSubscribedToMessageType(string messageTypeKey)
        {
            return new SuccessResponseDto() { Succesfull = await _brokerService.IsUserSubscribedToMessageType(GetUserIdAsString(), messageTypeKey) };
        }

        [HttpPost]
        public async Task<MessageWrapperDto> PublishMessage(string messageTypeId, string message)
        {
            return new MessageWrapperDto() { Message = await _brokerService.PublishMessage(messageTypeId, messageTypeId) };
        }

        [HttpPost]
        public async Task<SuccessResponseDto> SubscribeToMessageType(string messageTypeKey)
        {
            return new SuccessResponseDto() { Succesfull = await _brokerService.SubscribeToMessageType(GetUserIdAsString(), messageTypeKey) };
        }

        [HttpPost]
        public async Task<SuccessResponseDto> UnsubscribeFromMessageType(string messageTypeKey)
        {
            return new SuccessResponseDto() { Succesfull = await _brokerService.UnsubscribeFromMessageType(GetUserIdAsString(), messageTypeKey) };
        }

        private string GetUserIdAsString()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();
            if (userId == null || userId.Length > 5)
                return Guid.Empty.ToString();
            else
                return userId;
        }
    }
}
