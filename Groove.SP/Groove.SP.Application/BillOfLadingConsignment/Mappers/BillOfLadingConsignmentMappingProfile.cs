using Groove.SP.Application.BillOfLadingConsignment.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BillOfLadingConsignment.Mappers
{
    public class BillOfLadingConsignmentMappingProfile : MappingProfileBase<BillOfLadingConsignmentModel, BillOfLadingConsignmentViewModel>
    {
        public BillOfLadingConsignmentMappingProfile()
        {
            CreateMap<BillOfLadingConsignmentModel, BillOfLadingConsignmentViewModel>()
                .ReverseMap()
                .ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<BillOfLadingConsignmentViewModel, BillOfLadingConsignmentModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
