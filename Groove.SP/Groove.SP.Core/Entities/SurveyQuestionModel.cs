using Groove.SP.Core.Models;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class SurveyQuestionModel : Entity
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public int Sequence { get; set; }
        public SurveyQuestionType Type { get; set; }
        public int? StarRating { get; set; }
        public string LowValueLabel { get; set; }
        public string HighValueLabel { get; set; }
        public bool IsIncludeOpenEndedText { get; set; }
        public string PlaceHolderText { get; set; }
        public long SurveyId { get; set; }

        public SurveyModel Survey { get; set; }
        public ICollection<SurveyQuestionOptionModel> Options { get; set; }
        public ICollection<SurveyAnswerModel> Answers { get; set; }
    }
}