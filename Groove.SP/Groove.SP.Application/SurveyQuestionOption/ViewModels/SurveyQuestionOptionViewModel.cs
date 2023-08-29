using Groove.SP.Application.Common;
using Groove.SP.Application.SurveyQuestion.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.SurveyQuestionOption.ViewModels
{
    public class SurveyQuestionOptionViewModel : ViewModelBase<SurveyQuestionOptionModel>
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public long QuestionId { get; set; }

        public SurveyQuestionViewModel Question { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
