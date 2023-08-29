using Groove.SP.Application.Common;
using Groove.SP.Application.SurveyQuestion.ViewModels;
using Groove.SP.Application.SurveyQuestionOption.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.SurveyAnswer.ViewModels
{
    public class SurveyAnswerViewModel : ViewModelBase<SurveyAnswerModel>
    {
        public long Id { get; set; }
        public string AnswerText { get; set; }
        public int? AnswerNumeric { get; set; }
        public string Username { get; set; }
        public long QuestionId { get; set; }
        public long? OptionId { get; set; }

        public SurveyQuestionViewModel Question { get; set; }
        public SurveyQuestionOptionViewModel Option { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
