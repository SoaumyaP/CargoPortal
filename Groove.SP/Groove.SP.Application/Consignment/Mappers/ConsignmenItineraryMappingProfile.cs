using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Consignment.Mappers
{
    public class ConsignmentMappingItineraryProfile : MappingProfileBase<ConsignmentItineraryModel, ConsignmentItineraryViewModel>
    {
        public ConsignmentMappingItineraryProfile()
        {
            CreateMap<ConsignmentItineraryModel, ConsignmentItineraryViewModel>()
                .ReverseMap()
                .ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<ConsignmentItineraryViewModel, ConsignmentItineraryModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
