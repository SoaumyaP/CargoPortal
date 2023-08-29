using System.Linq;
using Groove.SP.Application.Mappers;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.BillOfLading.Mappers
{
    public class BillOfLadingMappingProfile : MappingProfileBase<BillOfLadingModel, BillOfLadingViewModel>
    {
        public BillOfLadingMappingProfile()
        {
            CreateMap<BillOfLadingModel, BillOfLadingViewModel>()
                .ReverseMap()
                .ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<BillOfLadingModel, QuickTrackBillOfLadingViewModel>().ReverseMap();

            CreateMap<BillOfLadingViewModel, BillOfLadingModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
