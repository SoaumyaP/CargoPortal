using Groove.SP.Application.Mappers;
using Groove.SP.Application.SurveyQuestionOption.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.SurveyQuestionOption.Mappers
{
    public class SurveyQuestionOptionMappingProfile : MappingProfileBase<SurveyQuestionOptionModel, SurveyQuestionOptionViewModel>
    {
        public SurveyQuestionOptionMappingProfile()
        {
            CreateMap<SurveyQuestionOptionViewModel, SurveyQuestionOptionModel>().ReverseMap();
        }
    }
}
