using Groove.SP.Application.Mappers;
using Groove.SP.Application.MasterBillOfLadingContact.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.MasterBillOfLadingContact.Mappers
{
    public class MasterBillOfLadingContactMappingProfile : MappingProfileBase<MasterBillOfLadingContactModel, MasterBillOfLadingContactViewModel>
    {
        public MasterBillOfLadingContactMappingProfile()
        {
            CreateMap<MasterBillOfLadingContactModel, MasterBillOfLadingContactViewModel>().ReverseMap()
                 .ForMember(src => src.RowVersion, opt => opt.Ignore());

            CreateMap<MasterBillOfLadingContactViewModel, MasterBillOfLadingContactModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
