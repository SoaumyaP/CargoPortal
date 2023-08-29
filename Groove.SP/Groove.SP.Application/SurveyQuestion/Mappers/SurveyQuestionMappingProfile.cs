using Groove.SP.Application.Mappers;
using Groove.SP.Application.SurveyQuestion.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.SurveyQuestion.Mappers
{
    public class SurveyQuestionMappingProfile : MappingProfileBase<SurveyQuestionModel, SurveyQuestionViewModel>
    {
        public SurveyQuestionMappingProfile()
        {
            CreateMap<SurveyQuestionViewModel, SurveyQuestionModel>()
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}