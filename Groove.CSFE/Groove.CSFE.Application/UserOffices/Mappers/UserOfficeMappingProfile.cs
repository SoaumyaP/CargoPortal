using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Application.UserOffices.ViewModels;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.UserOffices.Mappers
{
    public class UserOfficeMappingProfile : MappingProfileBase<UserOfficeModel, UserOfficeViewModel>
    {
        public UserOfficeMappingProfile()
        {
            CreateMap<UserOfficeModel, UserOfficeViewModel>().ReverseMap();
        }
    }
}
