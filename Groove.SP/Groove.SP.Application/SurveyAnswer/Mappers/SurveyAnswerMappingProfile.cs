using Groove.SP.Application.Mappers;
using Groove.SP.Application.SurveyAnswer.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.SurveyAnswer.Mappers
{
    public class SurveyAnswerMappingProfile : MappingProfileBase<SurveyAnswerModel, SurveyAnswerViewModel>
    {
        public SurveyAnswerMappingProfile()
        {
            CreateMap<SurveyAnswerViewModel, SurveyAnswerModel>()
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
