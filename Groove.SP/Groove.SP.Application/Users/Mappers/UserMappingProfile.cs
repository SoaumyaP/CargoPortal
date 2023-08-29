using Groove.SP.Application.Mappers;
using Groove.SP.Application.Users.ViewModels;
using Groove.SP.Core.Entities;
using System.Linq;

namespace Groove.SP.Application.Users.Mappers
{
    public class UserMappingProfile : MappingProfileBase<UserProfileModel, UserProfileViewModel>
    {
        public UserMappingProfile()
        {
            //UserRequest
            CreateMap<UserRequestModel, UserRequestViewModel>()
                .ForMember(opts => opts.CreatedDate, dest => dest.MapFrom(src => src.CreatedDate.Date));
            CreateMap<UserRequestViewModel, UserRequestModel>();

            //Role
            CreateMap<RoleModel, RoleViewModel>()
               .ForMember(s => s.PermissionIds, d => d.MapFrom(m => m.RolePermissions.Select(x => x.PermissionId)));

            //UserRole
            CreateMap<UserRoleModel, UserRoleViewModel>().ReverseMap();

            //UserProfile
            CreateMap<UserProfileModel, UserProfileViewModel>().ForMember(x => x.Permissions, x => x.Ignore()).ReverseMap();

            //UseTrackTrace
            CreateMap<UserAuditLogModel, UserAuditLogViewModel>().ReverseMap();
            CreateMap<UserAuditLogQueryModel, UserAuditLogViewModel>().ReverseMap();


        }
    }
}
