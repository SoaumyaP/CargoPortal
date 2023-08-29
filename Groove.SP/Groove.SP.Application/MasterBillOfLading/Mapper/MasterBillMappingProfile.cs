using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.MasterBillOfLading.Mapper
{
    public class MasterBillMappingProfile : MappingProfileBase<MasterBillOfLadingModel, MasterBillOfLadingViewModel>
    {
        public MasterBillMappingProfile()
        {
            CreateMap<MasterBillOfLadingModel, MasterBillOfLadingViewModel>();

            CreateMap<MasterBillOfLadingViewModel, MasterBillOfLadingModel>()
                .ForMember(src => src.RowVersion, dt => dt.Ignore());

            CreateMap<MasterBillOfLadingViewModel, MasterBillOfLadingModel>();

            CreateMap<UpdateMasterBillOfLadingViewModel, MasterBillOfLadingModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<CreateMasterBillOfLadingViewModel, MasterBillOfLadingModel>();
        }
    }
}
