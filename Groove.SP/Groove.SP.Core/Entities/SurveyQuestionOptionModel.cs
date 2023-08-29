using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class SurveyQuestionOptionModel : Entity
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public long QuestionId { get; set; }

        public SurveyQuestionModel Question { get; set; }
        public ICollection<SurveyAnswerModel> Answers { get; set; }
    }
}