using Groove.SP.Application.EmailSetting.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.EmailSetting.Mappers;
public class EmailSettingMappingProfile : MappingProfileBase<EmailSettingModel, EmailSettingViewModel>
{
    public EmailSettingMappingProfile()
    {
        CreateMap<EmailSettingModel, EmailSettingViewModel>()
            .ReverseMap()
            .ForMember(src => src.RowVersion, opt => opt.Ignore());
    }
}