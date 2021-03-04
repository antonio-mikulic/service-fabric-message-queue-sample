using AutoMapper;
using FabricMQ.Broker.Identity.Models;
using FabricMQ.Broker.Identity.Models.Dto;

namespace FabricMQ.Broker.Mapping
{

    public class CustomDtoMapper : Profile
    {
        public CustomDtoMapper()
        {
            // Users
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
            CreateMap<CreateOrUpdateUserDto, UserDto>();
            CreateMap<UserDto, CreateOrUpdateUserDto>();
            CreateMap<CreateOrUpdateUserDto, User>();
            CreateMap<User, CreateOrUpdateUserDto>();


            // Roles
            CreateMap<Role, RoleDto>();
            CreateMap<RoleDto, Role>();
            CreateMap<Role, CreateOrEditRoleDto>();
            CreateMap<CreateOrEditRoleDto, Role>();
            CreateMap<RoleDto, CreateOrEditRoleDto>();
            CreateMap<CreateOrEditRoleDto, RoleDto>();

        }
    }
}
