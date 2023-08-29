using Groove.SP.Application.GlobalIdMasterDialog.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.GlobalIdMasterDialog.Mappers
{
    public class GlobalIdMasterDialogMappingProfile : MappingProfileBase<GlobalIdMasterDialogModel, GlobalIdMasterDialogViewModel>
    {
        public GlobalIdMasterDialogMappingProfile()
        {
            CreateMap<GlobalIdMasterDialogModel, GlobalIdMasterDialogViewModel>()
                .ReverseMap();
        }
    }
}
