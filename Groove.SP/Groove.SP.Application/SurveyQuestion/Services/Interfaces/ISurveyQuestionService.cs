using Groove.SP.Application.Common;
using Groove.SP.Application.SurveyQuestion.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.SurveyQuestion.Services.Interfaces
{
    public interface ISurveyQuestionService : IServiceBase<SurveyQuestionModel, SurveyQuestionViewModel>
    {
        Task<QuestionPieChartReportViewModel<int>> GetPieChartReportAsync(long id);
        Task<QuestionBarChartReportViewModel<int>> GetBarChartReportAsync(long id);
        Task<DataSourceResult> SummaryReportSearchingAsync(DataSourceRequest request, long id);
        Task<IEnumerable<SurveyQuestionViewModel>> GetBySurveyIdAsync(long surveyId);
    }
}