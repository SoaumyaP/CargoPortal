using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BillOfLading.Mappers
{
    public class BillOfLadingItineraryMappingProfile : MappingProfileBase<BillOfLadingItineraryModel, BillOfLadingItineraryViewModel>
    {
        public BillOfLadingItineraryMappingProfile()
        {
            CreateMap<BillOfLadingItineraryModel, BillOfLadingItineraryViewModel>()
                .ReverseMap()
                .ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<BillOfLadingItineraryViewModel, BillOfLadingItineraryModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
