using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Consignment.Mappers
{
    public class ConsignmentMappingProfile : MappingProfileBase<ConsignmentModel, ConsignmentViewModel>
    {
        public ConsignmentMappingProfile()
        {
            CreateMap<ConsignmentModel, ConsignmentViewModel>()
                .ReverseMap();

            CreateMap<ConsignmentViewModel, ConsignmentModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
