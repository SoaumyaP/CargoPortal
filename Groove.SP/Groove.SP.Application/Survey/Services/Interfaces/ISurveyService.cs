using Groove.SP.Application.Common;
using Groove.SP.Application.Survey.ViewModels;
using Groove.SP.Application.SurveyAnswer.ViewModels;
using Groove.SP.Application.SurveyParticipant.ViewModels;
using Groove.SP.Application.SurveyQuestion.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.Survey.Services.Interfaces
{
    public interface ISurveyService : IServiceBase<SurveyModel, SurveyViewModel>
    {
        Task<SurveyViewModel> GetDetailAsync(long id);
        Task<IEnumerable<SurveyParticipantViewModel>> GetSurveyParticipantsAsync(IdentityInfo currentUser);
        Task<SurveyViewModel> PublishAsync(long surveyId, IdentityInfo currentUser, SurveyViewModel surveyViewModel);
        Task SubmitAsync(long surveyId, IdentityInfo currentUser, IEnumerable<SurveyAnswerViewModel> surveyAnswerViewModel);
        Task<IEnumerable<SurveyQuestionViewModel>> PreviewSurveyAsync(long surveyId, IdentityInfo currentUser);
        Task<IEnumerable<SurveyQuestionViewModel>> GetToSubmitAsync(long surveyId, IdentityInfo currentUser);
        Task<SurveyViewModel> UpdateAsync(SurveyViewModel survey, string userName);
        Task CloseAsync(long id, string userName);
        Task<int> CountParticipantAsync(long id, bool? isSubmitted);
    }
}