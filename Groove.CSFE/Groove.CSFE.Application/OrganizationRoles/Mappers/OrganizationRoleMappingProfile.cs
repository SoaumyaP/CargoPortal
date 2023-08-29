using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Application.OrganizationRoles.ViewModels;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.OrganizationRoles.Mappers
{
    public class OrganizationRoleMappingProfile : MappingProfileBase<OrganizationRoleModel, OrganizationRoleViewModel>
    {
        public OrganizationRoleMappingProfile()
        {
            CreateMap<OrganizationRoleModel, OrganizationRoleViewModel>().ReverseMap();

            CreateMap<OrganizationRoleViewModel, OrganizationRoleModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
