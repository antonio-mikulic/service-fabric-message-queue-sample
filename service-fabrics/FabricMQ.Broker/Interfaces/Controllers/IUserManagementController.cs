using System.Collections.Generic;
using System.Threading.Tasks;
using FabricMQ.Broker.Identity.Models;
using FabricMQ.Broker.Identity.Models.Dto;

namespace FabricMQ.Broker.Interfaces.Controllers
{
    public interface IUserManagementController
    {
        Task<List<UserDto>> GetAllUsers();
        Task<UserDto> GetUserByIdAsync(string userId);
        Task<UserDto> CreateUser(CreateOrUpdateUserDto dto);
        Task<UserDto> UpdateUser(CreateOrUpdateUserDto dto);
        Task DeleteUser(string userId);
        Task AddToRoleAsync(UserRoleInformationDto userRole);
        Task RemoveFromRoleAsync(UserRoleInformationDto userRole);
        Task<List<string>> GetRolesAsync(string userId);
        Task<bool> IsInRoleAsync(UserRoleInformationDto userRole);
        Task<List<UserDto>> GetUsersInRoleAsync(string roleName);

    }
}
