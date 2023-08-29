using Groove.SP.Application.Mappers;
using Groove.SP.Application.Survey.ViewModels;
using Groove.SP.Application.SurveyParticipant.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Survey.Mappers
{
    public class SurveyParticipantMappingProfile : MappingProfileBase<SurveyParticipantModel, SurveyParticipantViewModel>
    {
        public SurveyParticipantMappingProfile()
        {
            CreateMap<SurveyParticipantViewModel, SurveyParticipantModel>()
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}