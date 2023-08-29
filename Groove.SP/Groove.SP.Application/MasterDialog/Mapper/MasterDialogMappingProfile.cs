using System.Linq;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Application.MasterDialog.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Itinerary.Mappers
{
    public class MasterDialogMappingProfile : MappingProfileBase<MasterDialogModel, MasterDialogViewModel>
    {
        public MasterDialogMappingProfile()
        {
            CreateMap<MasterDialogViewModel, MasterDialogModel>()
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.CreatedDate, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
