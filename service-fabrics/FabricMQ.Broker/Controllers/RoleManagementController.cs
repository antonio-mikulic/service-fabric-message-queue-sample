using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FabricMQ.Broker.Database;
using FabricMQ.Broker.Helpers;
using FabricMQ.Broker.Identity.Models;
using FabricMQ.Broker.Identity.Models.Dto;
using FabricMQ.Broker.Interfaces.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FabricMQ.Broker.Controllers
{
    [Route("api/[controller]/[action]")]
    public class RoleManagementController : Controller, IRoleManagementController
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly IBrokerService _brokerService;

        public IDatabaseContext Context { get; }

        private IMapper ObjectMapper { get; set; }

        public const string NotificationKey = StaticSubscriptionNames.UserRoleNotificationString;

        public RoleManagementController(RoleManager<Role> roleManager, IBrokerService brokerService, IMapper mapper, IDatabaseContext context)
        {
            _roleManager = roleManager;
            _brokerService = brokerService;
            ObjectMapper = mapper;
            Context = context;
        }

        [HttpGet]
        public string GetRoleNotificationKey()
        {
            return NotificationKey;
        }

        [HttpPost]
        public async Task<List<RoleDto>> GetAllRoles()
        {
            var roles = await Context.Roles.GetAll();
            return ObjectMapper.Map<List<RoleDto>>(roles);
        }

        [HttpPost]
        public async Task<RoleDto> GetSingleRoleById(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            return ObjectMapper.Map<RoleDto>(role);
        }

        [HttpPost]
        public async Task<RoleDto> CreateRole([FromBody] CreateOrEditRoleDto input)
        {
            var foundNameInUse = await _roleManager.FindByNameAsync(input.Name);

            if (foundNameInUse != null)
                throw new Exception("Role with this name already exists!");

            var role = ObjectMapper.Map<Role>(input);
            await _roleManager.CreateAsync(role);

            await _brokerService.PublishMessage(NotificationKey, $"Succesfully created role {input.Name}");

            return ObjectMapper.Map<RoleDto>(input);
        }

        [HttpPost]
        public async Task<RoleDto> UpdateRole([FromBody] CreateOrEditRoleDto input)
        {
            var role = await _roleManager.FindByIdAsync(input.Id.ToString());

            if (role == null)
                throw new Exception("Role not found!");

            role = ObjectMapper.Map<Role>(input);

            await _roleManager.UpdateAsync(role);

            await _brokerService.PublishMessage(NotificationKey, $"Succesfully updated role {input.Name}");

            return ObjectMapper.Map<RoleDto>(input);
        }

        [HttpPost]
        public async Task DeleteRoleById(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            await _roleManager.DeleteAsync(role);

            await _brokerService.PublishMessage(NotificationKey, $"Succesfully deleted role {role.Name}");
        }
    }
}