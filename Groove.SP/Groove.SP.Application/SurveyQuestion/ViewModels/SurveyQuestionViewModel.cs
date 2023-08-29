using Groove.SP.Application.Common;
using Groove.SP.Application.Survey.ViewModels;
using Groove.SP.Application.SurveyQuestionOption.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.Collections.Generic;

namespace Groove.SP.Application.SurveyQuestion.ViewModels
{
    public class SurveyQuestionViewModel : ViewModelBase<SurveyQuestionModel>
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public int Sequence { get; set; }
        public SurveyQuestionType Type { get; set; }
        public string TypeName => EnumHelper<SurveyQuestionType>.GetDisplayName(Type);
        public int? StarRating { get; set; }
        public string LowValueLabel { get; set; }
        public string HighValueLabel { get; set; }
        public bool IsIncludeOpenEndedText { get; set; }
        public string PlaceHolderText { get; set; }
        public long SurveyId { get; set; }
        public bool IsSubmitted { get; set; }
        public SurveyViewModel Survey { get; set; }
        public IEnumerable<SurveyQuestionOptionViewModel> Options { get; set; }
        //public IEnumerable<SurveyAnswerViewModel> Answers { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
