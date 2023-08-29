using Groove.SP.Application.Mappers;
using Groove.SP.Application.Survey.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Survey.Mappers
{
    public class SurveyMappingProfile : MappingProfileBase<SurveyModel, SurveyViewModel>
    {
        public SurveyMappingProfile()
        {
            CreateMap<SurveyViewModel, SurveyModel>()
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}