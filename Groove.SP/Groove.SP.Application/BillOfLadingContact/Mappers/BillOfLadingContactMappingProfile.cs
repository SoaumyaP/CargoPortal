using Groove.SP.Application.BillOfLadingContact.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BillOfLadingContact.Mappers
{
    public class BillOfLadingContactMappingProfile : MappingProfileBase<BillOfLadingContactModel, BillOfLadingContactViewModel>
    {
        public BillOfLadingContactMappingProfile()
        {
            CreateMap<BillOfLadingContactModel, BillOfLadingContactViewModel>().ReverseMap().ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<BillOfLadingContactViewModel, BillOfLadingContactModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
