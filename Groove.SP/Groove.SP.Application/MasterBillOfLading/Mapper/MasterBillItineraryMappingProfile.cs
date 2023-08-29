using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.MasterBillOfLading.Mapper
{
    public class MasterBillItineraryMappingProfile : MappingProfileBase<MasterBillOfLadingItineraryModel, MasterBillOfLadingItineraryViewModel>
    {
        public MasterBillItineraryMappingProfile()
        {
            CreateMap<MasterBillOfLadingItineraryModel, MasterBillOfLadingItineraryViewModel>()
                .ReverseMap()
                .ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<MasterBillOfLadingItineraryViewModel, MasterBillOfLadingItineraryModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
