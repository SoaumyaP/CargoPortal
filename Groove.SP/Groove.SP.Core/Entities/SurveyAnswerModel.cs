namespace Groove.SP.Core.Entities
{
    public class SurveyAnswerModel : Entity
    {
        public long Id { get; set; }
        public string AnswerText { get; set; }
        public int? AnswerNumeric { get; set; }
        public string Username { get; set; }
        public long QuestionId { get; set; }
        public long? OptionId { get; set; }

        public SurveyQuestionModel Question { get; set; }
        public SurveyQuestionOptionModel Option { get; set; }
    }
}