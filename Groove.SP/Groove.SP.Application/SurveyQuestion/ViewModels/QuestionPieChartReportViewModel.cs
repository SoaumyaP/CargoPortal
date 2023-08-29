using System.Collections.Generic;

namespace Groove.SP.Application.SurveyQuestion.ViewModels
{
    public class QuestionPieChartReportViewModel<T>
    {
        public List<PieChartCategory<T>> Categories { get; set; }
        public List<OtherAnswerItemViewModel> OtherAnswers { get; set; }
    }

    public class QuestionBarChartReportViewModel<T> : BarChartCategory<T>
    {
        public List<BarChartCategory<T>> Categories { get; set; }
        public List<OtherAnswerItemViewModel> OtherAnswers { get; set; }
    }
    public class BarChartCategory<T>
    {
        public string Category { get; set; }
        public T Value { get; set; }
    }

    public class PieChartCategory<T>
    {
        public string Category { get; set; }
        public T Value { get; set; }
    }

    public class OtherAnswerItemViewModel
    {
        public string AnswerText { get; set; }
        public int? AnswerNumeric { get; set; }
    }
}