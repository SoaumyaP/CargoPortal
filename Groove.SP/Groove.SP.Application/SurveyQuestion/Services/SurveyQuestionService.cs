using AutoMapper.QueryableExtensions;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.SurveyAnswer.ViewModels;
using Groove.SP.Application.SurveyQuestion.Services.Interfaces;
using Groove.SP.Application.SurveyQuestion.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.SurveyQuestion.Services
{
    public class SurveyQuestionService : ServiceBase<SurveyQuestionModel, SurveyQuestionViewModel>, ISurveyQuestionService
    {
        private readonly IRepository<SurveyAnswerModel> _surveyAnswerRepository;

        public SurveyQuestionService(IUnitOfWorkProvider unitOfWorkProvider, IRepository<SurveyAnswerModel> surveyAnswerRepository) : base(unitOfWorkProvider)
        {
            _surveyAnswerRepository = surveyAnswerRepository;
        }

        public async Task<IEnumerable<SurveyQuestionViewModel>> GetBySurveyIdAsync(long surveyId)
        {
            var models = await Repository.QueryAsNoTracking(x => x.SurveyId == surveyId, orderBy: x => x.OrderBy(y => y.Sequence)).ToListAsync();

            return Mapper.Map<IEnumerable<SurveyQuestionViewModel>>(models);
        }

        public async Task<QuestionPieChartReportViewModel<int>> GetPieChartReportAsync(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(x => x.Id == id, includes: i => i.Include(y => y.Answers).Include(y => y.Options));

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            QuestionPieChartReportViewModel<int> result = new();

            if (model.Type == SurveyQuestionType.RatingScale)
            {
                result.Categories = Enumerable.Range(1, model.StarRating ?? 0).Select(star => new PieChartCategory<int>
                {
                    Category = star.ToString(),
                    Value = model.Answers.Count(a => a.AnswerNumeric == star)
                }
                ).ToList();
            }
            else
            {
                if (model.Options.Any())
                {
                    result.Categories = model.Options.Select(x => new PieChartCategory<int>
                    {
                        Category = x.Content,
                        Value = model.Answers.Count(a => a.OptionId == x.Id)
                    }
                    ).ToList();
                }

                // Other open-ended answers
                if (model.Answers.Any(x => x.OptionId == null))
                {
                    var other = new PieChartCategory<int>
                    {
                        Category = "Other",
                        Value = model.Answers.Count(x => x.OptionId == null)
                    };
                    result.Categories.Add(other);

                    result.OtherAnswers = model.Answers.Where(x => x.OptionId == null).Select(x => new OtherAnswerItemViewModel
                    {
                        AnswerText = x.AnswerText
                    }).ToList();
                }
            }

            return result;
        }

        public async Task<QuestionBarChartReportViewModel<int>> GetBarChartReportAsync(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(x => x.Id == id, includes: i => i.Include(y => y.Answers).Include(y => y.Options));

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            QuestionBarChartReportViewModel<int> result = new();

            if (model.Options.Any())
            {
                result.Categories = model.Options.Select(x => new BarChartCategory<int>
                {
                    Category = x.Content,
                    Value = model.Answers.Count(a => a.OptionId == x.Id)
                }).ToList();
            }

            // Other open-ended answers
            if (model.Answers.Any(x => x.OptionId == null))
            {
                var other = new BarChartCategory<int>
                {
                    Category = "Other",
                    Value = model.Answers.Count(x => x.OptionId == null)
                };
                result.Categories.Add(other);

                result.OtherAnswers = model.Answers.Where(x => x.OptionId == null).Select(x => new OtherAnswerItemViewModel
                {
                    AnswerText = x.AnswerText
                }).ToList();
            }

            return result;
        }

        public async Task<DataSourceResult> SummaryReportSearchingAsync(DataSourceRequest request, long id)
        {
            var query = _surveyAnswerRepository.QueryAsNoTracking(x => x.QuestionId == id);
            return await query.ProjectTo<SurveyAnswerViewModel>(Mapper.ConfigurationProvider)
                .ToDataSourceResultAsync(request);
        }
    }
}