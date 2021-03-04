using System.Collections.Generic;
using System.Threading.Tasks;
using FabricMQ.Broker.Identity.Models.Dto;

namespace FabricMQ.Broker.Interfaces.Controllers
{
    public interface IRoleManagementController
    {
        Task<List<RoleDto>> GetAllRoles();
        Task<RoleDto> GetSingleRoleById(string roleId);
        Task<RoleDto> CreateRole(CreateOrEditRoleDto input);
        Task<RoleDto> UpdateRole(CreateOrEditRoleDto input);
        Task DeleteRoleById(string roleId);
    }
}
