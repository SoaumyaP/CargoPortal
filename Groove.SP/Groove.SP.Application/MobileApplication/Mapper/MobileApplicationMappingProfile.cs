using Groove.SP.Application.Mappers;
using Groove.SP.Application.MobileApplication.ViewModels;
using Groove.SP.Core.Entities.Mobile;

namespace Groove.SP.Application.MobileApplication.Mapper
{
    public class MobileApplicationMappingProfile : MappingProfileBase<MobileApplicationModel, MobileApplicationViewModel>
    {
        public MobileApplicationMappingProfile()
        {
            CreateMap<MobileApplicationModel, MobileApplicationViewModel>();

            CreateMap<MobileApplicationModel, MobileApplicationMobileModel>();
            CreateMap<MobileApplicationViewModel, MobileApplicationMobileModel>();


        }
    }
}
