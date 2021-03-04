using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UserManagementController : Controller, IUserManagementController
    {
        private readonly UserManager<User> _userManager;
        private readonly IBrokerService _brokerService;

        public IDatabaseContext Context { get; }

        private IMapper ObjectMapper { get; set; }

        public const string NotificationKey = StaticSubscriptionNames.UserManagementNotificationString;

        public UserManagementController(UserManager<User> userManager, IMapper mapper, IBrokerService brokerService, IDatabaseContext context)
        {
            _userManager = userManager;
            ObjectMapper = mapper;
            _brokerService = brokerService;
            Context = context;
        }

        [HttpGet]
        public string GetUSerManagementNotificationKey()
        {
            return NotificationKey;
        }

        [HttpPost]
        public async Task<List<UserDto>> GetAllUsers()
        {
            var users = await Context.Users.GetAll();

            var usersDto = new List<UserDto>();

            foreach (var user in users)
            {
                var dto = ObjectMapper.Map<UserDto>(user);
                var roleNames = await _userManager.GetRolesAsync(user);
                dto.RoleNames = roleNames.ToList();

                usersDto.Add(dto);
            }

            return usersDto;
        }

        [HttpPost]
        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var roleNames = await _userManager.GetRolesAsync(user);

            var dto = ObjectMapper.Map<UserDto>(user);
            dto.RoleNames = roleNames.ToList();

            return dto;
        }

        [HttpPost]
        public async Task<UserDto> CreateUser([FromBody] CreateOrUpdateUserDto dto)
        {
            var user = ObjectMapper.Map<User>(dto);
            user.EmailConfirmed = false;

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                throw new AggregateException(result.Errors != null ? result.Errors.Select(s => new Exception(s.Description)).ToList() : new List<Exception>());
            }

            foreach (var roleName in dto.RoleNames)
                await _userManager.AddToRoleAsync(user, roleName);

            await _brokerService.PublishMessage(NotificationKey, $"Succesfully created user {dto.Name}");

            return ObjectMapper.Map<UserDto>(user);
        }


        [HttpPost]
        public async Task<UserDto> UpdateUser([FromBody] CreateOrUpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.Id.ToString());
            if (user == null)
                throw new Exception("Could not find user");

            user = ObjectMapper.Map<User>(dto);

            await _userManager.UpdateAsync(user);

            var foundRoles = await _userManager.GetRolesAsync(user);

            var addRoles = foundRoles.Where(s => !dto.RoleNames.Contains(s));
            var removeRoles = dto.RoleNames.Where(s => foundRoles.Contains(s));

            foreach (var roleName in addRoles)
                await _userManager.AddToRoleAsync(user, roleName);

            foreach (var roleName in removeRoles)
                await _userManager.RemoveFromRoleAsync(user, roleName);

            await _brokerService.PublishMessage(NotificationKey, $"Succesfully updated user {dto.Name}");

            return ObjectMapper.Map<UserDto>(user);
        }

        [HttpPost]
        public async Task DeleteUser(string userId)
        {
            var found = await _userManager.FindByIdAsync(userId);
            await _userManager.DeleteAsync(found);
            await _brokerService.PublishMessage(NotificationKey, $"Succesfully deleted user {found.Name}");

        }

        [HttpPost]
        public async Task AddToRoleAsync(UserRoleInformationDto userRole)
        {
            var found = await _userManager.FindByIdAsync(userRole.UserId);
            await _userManager.AddToRoleAsync(found, userRole.RoleName);
        }

        [HttpPost]
        public async Task RemoveFromRoleAsync(UserRoleInformationDto userRole)
        {
            var found = await _userManager.FindByIdAsync(userRole.UserId);
            await _userManager.RemoveFromRoleAsync(found, userRole.RoleName);
        }

        [HttpPost]
        public async Task<List<string>> GetRolesAsync(string userId)
        {
            var found = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(found);
            return roles.ToList();
        }

        [HttpPost]
        public async Task<bool> IsInRoleAsync(UserRoleInformationDto userRole)
        {
            var found = await _userManager.FindByIdAsync(userRole.UserId);
            return await _userManager.IsInRoleAsync(found, userRole.RoleName);
        }

        [HttpPost]
        public async Task<List<UserDto>> GetUsersInRoleAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return ObjectMapper.Map<List<UserDto>>(users);
        }
    }
}
