using Groove.SP.Application.BulkFulfillment.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Application.OrganizationPreference.ViewModels;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.OrganizationPreference.Mappers
{
    public class OrganizationPreferenceMappingProfile : MappingProfileBase<OrganizationPreferenceModel, OrganizationPreferenceViewModel>
    {
        public OrganizationPreferenceMappingProfile()
        {
            CreateMap<POFulfillmentOrderViewModel, OrganizationPreferenceViewModel>()
                .ForMember(c => c.Id, opt => opt.Ignore());

            CreateMap<BulkFulfillmentOrderViewModel, OrganizationPreferenceViewModel>()
                .ForMember(c => c.Id, opt => opt.Ignore());

            CreateMap<OrganizationPreferenceViewModel, OrganizationPreferenceModel>();
            CreateMap<OrganizationPreferenceModel, OrganizationPreferenceViewModel>();
        }
    }
}
