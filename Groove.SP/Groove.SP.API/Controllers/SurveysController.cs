using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Survey.Services.Interfaces;
using Groove.SP.Application.Survey.ViewModels;
using Groove.SP.Application.SurveyAnswer.ViewModels;
using Groove.SP.Application.SurveyQuestion.Services.Interfaces;
using Groove.SP.Core.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveysController : ControllerBase
    {
        private readonly ISurveyListService _surveyListService;
        private readonly ISurveyService _surveyService;
        private readonly ISurveyQuestionService _surveyQuestionService;
        public SurveysController(
           ISurveyListService surveyListService,
           ISurveyService surveyService,
           ISurveyQuestionService surveyQuestionService
            )
        {
            _surveyListService = surveyListService;
            _surveyService = surveyService;
            _surveyQuestionService = surveyQuestionService;
        }
        
        [HttpGet]
        [Route("statistics/sent-to-you")]
        public async Task<IActionResult> GetSurveySentToUsersAsync()
        {
            var viewModels = await _surveyService.GetSurveyParticipantsAsync(CurrentUser);
            return Ok(viewModels);
        }

        [HttpPut]
        [Route("{surveyId}/publish")]
        [AppAuthorize(AppPermissions.Organization_SurveyDetail_Edit)]
        public async Task<IActionResult> PublishAsync(long surveyId, SurveyViewModel surveyViewModel)
        {
            var survey = await _surveyService.PublishAsync(surveyId, CurrentUser, surveyViewModel);
            return Ok(survey);
        }

        [HttpPost]
        [Route("{surveyId}/submit")]
        public async Task<IActionResult> SubmitAsync(long surveyId, IEnumerable<SurveyAnswerViewModel> surveyAnswerModel)
        {
            await _surveyService.SubmitAsync(surveyId, CurrentUser, surveyAnswerModel);
            return Ok();
        }

        [HttpGet]
        [Route("{surveyId}/question-answer")]
        public async Task<IActionResult> PreviewOrSubmitAsync(long surveyId, string mode)
        {
            if (mode.Equals("preview", System.StringComparison.OrdinalIgnoreCase))
            {
                var viewModels = await _surveyService.PreviewSurveyAsync(surveyId, CurrentUser);
                if (viewModels.Count() == 0)
                {
                    return NotFound();
                }
                return Ok(viewModels);
            }
            else if (mode.Equals("submit", System.StringComparison.OrdinalIgnoreCase))
            {
                var viewModels = await _surveyService.GetToSubmitAsync(surveyId, CurrentUser);
                if (viewModels.Count() == 0)
                {
                    return NotFound();
                }
                return Ok(viewModels);
            }

            return NotFound(); ;
        }

        [HttpGet]
        [Route("{surveyId}/questions")]
        [AppAuthorize(AppPermissions.Organization_SurveyDetail)]
        public async Task<IActionResult> GetQuestionListAsync(long surveyId)
        {
            var result = await _surveyQuestionService.GetBySurveyIdAsync(surveyId);
            return Ok(result);
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Organization_SurveyList)]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _surveyListService.GetListAsync(request, CurrentUser);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Organization_SurveyList)]
        public async Task<IActionResult> GetAsync(long id)
        {
            var viewModel = await _surveyService.GetDetailAsync(id);
            return new JsonResult(viewModel);
        }

        [HttpPost]
        [AppAuthorize(AppPermissions.Organization_SurveyDetail_Add)]
        public async Task<IActionResult> CreateAsync([FromBody] SurveyViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var viewModel = await _surveyService.CreateAsync(model);
            return new JsonResult(viewModel);
        }

        [HttpPut]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Organization_SurveyDetail_Edit)]
        public async Task<IActionResult> UpdateAsync([FromBody] SurveyViewModel model)
        {
            var viewModel = await _surveyService.UpdateAsync(model, CurrentUser.Username);
            return new JsonResult(viewModel);
        }

        [HttpPut]
        [Route("{id}/close")]
        [AppAuthorize(AppPermissions.Organization_SurveyDetail_Edit)]
        public async Task<IActionResult> CloseAsync(long id)
        {
            await _surveyService.CloseAsync(id, CurrentUser.Username);
            return Ok();
        }

        #region report
        [HttpGet]
        [Route("{id}/participantCount")]
        [AppAuthorize(AppPermissions.Organization_SurveyDetail)]
        public async Task<IActionResult> CountParticipantAsync(long id, [FromQuery] bool? isSubmitted)
        {
            var res = await _surveyService.CountParticipantAsync(id, isSubmitted);
            return new JsonResult(res);
        }
        #endregion
    }
}
