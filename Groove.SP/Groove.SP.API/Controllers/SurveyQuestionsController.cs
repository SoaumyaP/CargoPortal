using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.SurveyQuestion.Services.Interfaces;
using Groove.SP.Core.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyQuestionsController : ControllerBase
    {
        private readonly ISurveyQuestionService _surveyQuestionService;

        public SurveyQuestionsController(ISurveyQuestionService surveyQuestionService)
        {
            _surveyQuestionService = surveyQuestionService;
        }

        [HttpGet]
        [Route("{questionId}/pie-chart")]
        [AppAuthorize(AppPermissions.Organization_SurveyDetail)]
        public async Task<IActionResult> GetPieChartReportAsync(long questionId)
        {
            var result = await _surveyQuestionService.GetPieChartReportAsync(questionId);
            return Ok(result);
        }

        [HttpGet]
        [Route("{questionId}/bar-chart")]
        [AppAuthorize(AppPermissions.Organization_SurveyDetail)]
        public async Task<IActionResult> GetBarChartReportAsync(long questionId)
        {
            var result = await _surveyQuestionService.GetBarChartReportAsync(questionId);
            return Ok(result);
        }

        [HttpGet]
        [Route("{questionId}/answer-summary")]
        [AppAuthorize(AppPermissions.Organization_SurveyDetail)]
        public async Task<IActionResult> SearchAnswerSummaryAsync([DataSourceRequest] DataSourceRequest request, long questionId)
        {
            var searchingData = await _surveyQuestionService.SummaryReportSearchingAsync(request, questionId);
            return new JsonResult(searchingData);
        }
    }
}