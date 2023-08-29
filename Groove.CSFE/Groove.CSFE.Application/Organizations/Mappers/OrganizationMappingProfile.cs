using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Linq;

namespace Groove.CSFE.Application.Organizations.Mappers
{
    public class OrganizationMappingProfile : MappingProfileBase<OrganizationModel, OrganizationViewModel>
    {
        public OrganizationMappingProfile()
        {
            CreateMap<OrganizationModel, OrganizationViewModel>()
                .ReverseMap();

            CreateMap<OrganizationModel, OrganizationReferenceDataViewModel>();

            CreateMap<SupplierViewModel, OrganizationModel>();

            CreateMap<CustomerRelationshipViewModel, CustomerRelationshipModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<OrganizationModel, OrganizationCodeViewModel>();

            CreateMap<BulkInsertOrganizationViewModel, OrganizationModel>();
        }
    }
}
