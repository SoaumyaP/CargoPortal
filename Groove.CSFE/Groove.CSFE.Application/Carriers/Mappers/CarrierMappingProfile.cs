using Groove.CSFE.Application.Carriers.ViewModels;
using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.Carriers.Mappers
{
    public class CarrierMappingProfile : MappingProfileBase<CarrierModel, CarrierViewModel>
    {
        public CarrierMappingProfile()
        {
            CreateMap<CarrierModel, CarrierViewModel>();

            CreateMap<CarrierViewModel, CarrierModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<CarrierQueryModel, CarrierListViewModel>();
        }
    }
}
