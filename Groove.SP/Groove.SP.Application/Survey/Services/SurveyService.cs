using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Survey.Services.Interfaces;
using Groove.SP.Application.Survey.ViewModels;
using Groove.SP.Application.SurveyQuestion.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Groove.SP.Application.Survey.BackgroundJobs;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Application.SurveyAnswer.ViewModels;
using Groove.SP.Application.SurveyParticipant.ViewModels;

namespace Groove.SP.Application.Survey.Services
{
    public class SurveyService : ServiceBase<SurveyModel, SurveyViewModel>, ISurveyService
    {
        private readonly IRepository<SurveyQuestionModel> _surveyQuestionRepository;
        private readonly IRepository<SurveyAnswerModel> _surveyAnswerRepository;
        private readonly IRepository<SurveyParticipantModel> _surveyParticipantRepository;
        private readonly IRepository<UserRoleModel> _userRoleRepository;
        private readonly IQueuedBackgroundJobs _queuedBackgroundJobs;
        private readonly IRepository<UserProfileModel> _userProfileRepository;
        private readonly ICSFEApiClient _csfeApiClient;

        public SurveyService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IRepository<SurveyQuestionModel> surveyQuestionRepository,
            IRepository<SurveyAnswerModel> surveyAnswerRepository,
            IRepository<UserRoleModel> userRoleRepository,
            IQueuedBackgroundJobs queuedBackgroundJobs,
            IRepository<UserProfileModel> userProfileRepository,
            IRepository<SurveyParticipantModel> surveyParticipantRepository,
            ICSFEApiClient csfeApiClient
            ) : base(unitOfWorkProvider)
        {
            _surveyQuestionRepository = surveyQuestionRepository;
            _surveyAnswerRepository = surveyAnswerRepository;
            _userRoleRepository = userRoleRepository;
            _queuedBackgroundJobs = queuedBackgroundJobs;
            _userProfileRepository = userProfileRepository;
            _surveyParticipantRepository = surveyParticipantRepository;
            _csfeApiClient = csfeApiClient;
        }

        public async Task<SurveyViewModel> PublishAsync(long surveyId, IdentityInfo currentUser, SurveyViewModel surveyViewModel)
        {
            UnitOfWork.BeginTransaction();
            var newSurvey = new SurveyViewModel();
            if (surveyId == 0)
            {
                surveyViewModel.Audit(currentUser.Username);
                newSurvey = await CreateAsync(surveyViewModel);
            }

            var surveyModel = await Repository.GetAsync(c => c.Id == (surveyId == 0 ? newSurvey.Id : surveyId) && c.Status == SurveyStatus.Draft, null, c => c.Include(c => c.Participants));

            if (surveyModel == null)
            {
                return null;
            }

            surveyModel.Status = SurveyStatus.Published;
            surveyModel.PublishedDate = DateTime.UtcNow;

            switch (surveyModel.ParticipantType)
            {
                case SurveyParticipantType.UserRole:
                    var userRoleModel = await _userRoleRepository.QueryAsNoTracking(c => c.RoleId == (int)surveyModel.UserRole, null, c => c.Include(c => c.User)).ToListAsync();
                    var usersProfile = userRoleModel.Select(c => c.User).ToList();
                    if (usersProfile.Any())
                    {
                        foreach (var user in usersProfile)
                        {
                            var surveyParticipantModel = new SurveyParticipantModel()
                            {
                                Username = user.Email.Trim(),
                                SurveyId = surveyModel.Id,
                            };
                            surveyParticipantModel.Audit(currentUser.Username);
                            surveyModel.Participants.Add(surveyParticipantModel);
                        }
                    }
                    break;

                case SurveyParticipantType.Organization:
                    if (surveyModel.OrganizationType == OrganizationType.Principal)
                    {
                        if (surveyModel.SendToUser == SurveySendToUserType.User)
                        {
                            var orgIds = surveyModel.SpecifiedOrganization?.Split(";").Select(s => Int64.TryParse(s, out long n) ? n : (long?)null).ToList();
                            if (surveyModel.IsIncludeAffiliate)
                            {
                                var affiliateOrgs = await _csfeApiClient.GetAffiliatesByOrgIdsAsync(surveyModel.SpecifiedOrganization);
                                var affiliateIds = affiliateOrgs.Select(c => (long?)c.Id);
                                orgIds.AddRange(affiliateIds);
                            }
                            await CreateParticipants(orgIds, surveyModel, currentUser);
                        }

                        if (surveyModel.SendToUser == SurveySendToUserType.UserInRelationship)
                        {
                            var customerRelationships = await _csfeApiClient.GetRelationshipAsync(surveyModel.SpecifiedOrganization, "principal");
                            var suplierIds = customerRelationships.Select(c => c.SupplierId).ToList();

                            if (surveyModel.IsIncludeAffiliate)
                            {
                                var affiliateOrgs = await _csfeApiClient.GetAffiliatesByOrgIdsAsync(string.Join(";", suplierIds));
                                var affiliateIds = affiliateOrgs.Select(c => (long?)c.Id);
                                suplierIds.AddRange(affiliateIds);
                            }
                            await CreateParticipants(suplierIds, surveyModel, currentUser);
                        }
                    }

                    if (surveyModel.OrganizationType == OrganizationType.General)
                    {
                        if (surveyModel.SendToUser == SurveySendToUserType.User)
                        {
                            var orgIds = surveyModel.SpecifiedOrganization?.Split(";").Select(s => Int64.TryParse(s, out long n) ? n : (long?)null).ToList();
                            if (surveyModel.IsIncludeAffiliate)
                            {
                                var affiliateOrgs = await _csfeApiClient.GetAffiliatesByOrgIdsAsync(surveyModel.SpecifiedOrganization);
                                var affiliateIds = affiliateOrgs.Select(c => (long?)c.Id);
                                orgIds.AddRange(affiliateIds);
                            }
                            await CreateParticipants(orgIds, surveyModel, currentUser);
                        }

                        if (surveyModel.SendToUser == SurveySendToUserType.UserInRelationship)
                        {
                            var customerRelationships = await _csfeApiClient.GetRelationshipAsync(surveyModel.SpecifiedOrganization, "General");
                            var customerIds = customerRelationships.Select(c => c.CustomerId).ToList();

                            if (surveyModel.IsIncludeAffiliate)
                            {
                                var affiliateOrgs = await _csfeApiClient.GetAffiliatesByOrgIdsAsync(string.Join(";", customerIds));
                                var affiliateIds = affiliateOrgs.Select(c => (long?)c.Id);
                                customerIds.AddRange(affiliateIds);
                            }
                            await CreateParticipants(customerIds, surveyModel, currentUser);
                        }
                    }

                    if (surveyModel.OrganizationType == OrganizationType.Agent)
                    {
                        var orgIds = surveyModel.SpecifiedOrganization?.Split(";").Select(s => Int64.TryParse(s, out long n) ? n : (long?)null).ToList();
                        if (surveyModel.IsIncludeAffiliate)
                        {
                            var affiliateOrgs = await _csfeApiClient.GetAffiliatesByOrgIdsAsync(surveyModel.SpecifiedOrganization);
                            var affiliateIds = affiliateOrgs.Select(c => (long?)c.Id);
                            orgIds.AddRange(affiliateIds);
                        }
                        await CreateParticipants(orgIds, surveyModel, currentUser);
                    }
                    break;

                case SurveyParticipantType.SpecifiedUser:
                    var participants = surveyModel.SpecifiedUser?.Split(",");
                    if (participants?.Length > 0)
                    {
                        foreach (var participant in participants)
                        {
                            var surveyParticipantModel = new SurveyParticipantModel()
                            {
                                Username = participant.Trim(),
                                SurveyId = surveyModel.Id,
                            };
                            surveyParticipantModel.Audit(currentUser.Username);
                            surveyModel.Participants.Add(surveyParticipantModel);
                        }
                    }
                    break;
                default:
                    break;
            }

            await UnitOfWork.SaveChangesAsync();
            UnitOfWork.CommitTransaction();

            _queuedBackgroundJobs.Enqueue<SurveyNotificationJob>(c => c.SendEmailAsync(surveyModel.Id));
            _queuedBackgroundJobs.Enqueue<SurveyNotificationJob>(c => c.PushNotificationAsync(surveyModel.Id));

            return newSurvey;
        }

        public async Task SubmitAsync(long surveyId, IdentityInfo currentUser, IEnumerable<SurveyAnswerViewModel> viewModels)
        {
            var surveyParticipant = await _surveyParticipantRepository.GetAsync(c => c.SurveyId == surveyId && c.Username == currentUser.Username && c.IsSubmitted == false);
            if (surveyParticipant == null)
            {
                return;
            }
            UnitOfWork.BeginTransaction();

            surveyParticipant.IsSubmitted = true;
            surveyParticipant.SubmittedOn = DateTime.UtcNow;
            await UnitOfWork.SaveChangesAsync();

            var surveyAnswerModels = new List<SurveyAnswerModel>();
            foreach (var viewModel in viewModels)
            {
                viewModel.Audit(currentUser.Username);
                var surveyAnswerModel = Mapper.Map<SurveyAnswerModel>(viewModel);
                surveyAnswerModels.Add(surveyAnswerModel);
            }
            await _surveyAnswerRepository.AddRangeAsync(surveyAnswerModels.ToArray());
            await UnitOfWork.SaveChangesAsync();

            UnitOfWork.CommitTransaction();
        }

        public async Task CreateParticipants(List<long?> orgIds, SurveyModel surveyModel, IdentityInfo currentUser)
        {
            var users = await _userProfileRepository.QueryAsNoTracking(
                                                x => orgIds.Contains(x.OrganizationId)
                                                && (x.Status == UserStatus.Active || x.Status == UserStatus.Inactive || x.Status == UserStatus.WaitForConfirm)).ToListAsync();

            foreach (var user in users)
            {
                var surveyParticipantModel = new SurveyParticipantModel()
                {
                    Username = user.Email.Trim(),
                    SurveyId = surveyModel.Id,
                };
                surveyParticipantModel.Audit(currentUser.Username);
                surveyModel.Participants.Add(surveyParticipantModel);
            }
        }

        public async Task<IEnumerable<SurveyQuestionViewModel>> PreviewSurveyAsync(long surveyId, IdentityInfo currentUser)
        {
            var surveyQuestions = await _surveyQuestionRepository.QueryAsNoTracking(
                c => c.SurveyId == surveyId && c.Survey.Status == SurveyStatus.Draft && c.Survey.CreatedBy == currentUser.Email,
                c => c.OrderBy(c => c.Sequence), c => c.Include(c => c.Options).Include(c => c.Survey)).ToListAsync();
            return Mapper.Map<IEnumerable<SurveyQuestionViewModel>>(surveyQuestions);
        }

        public async Task<IEnumerable<SurveyQuestionViewModel>> GetToSubmitAsync(long surveyId, IdentityInfo currentUser)
        {
            var surveyParticipant = await _surveyParticipantRepository.GetAsNoTrackingAsync(c =>
            c.SurveyId == surveyId && c.Username == currentUser.Username
            , null,
                c => c.Include(c => c.Survey).ThenInclude(c => c.Questions).ThenInclude(c => c.Options)
                );

            if (surveyParticipant == null)
            {
                return new List<SurveyQuestionViewModel>();
            }


            if (surveyParticipant.Survey.Status == SurveyStatus.Closed)
            {
                throw new AppValidationException($"SurveyIsNoLongerActive#Survey is no longer active");
            }

            var data = Mapper.Map<List<SurveyQuestionViewModel>>(surveyParticipant.Survey.Questions);
            data = data.OrderBy(c => c.Sequence).ToList();
            data[0].IsSubmitted = surveyParticipant.IsSubmitted;
            return data;
        }

        public async Task<IEnumerable<SurveyParticipantViewModel>> GetSurveyParticipantsAsync(IdentityInfo currentUser)
        {
            var surveyParticipants = await _surveyParticipantRepository.QueryAsNoTracking(c => c.Username == currentUser.Username && c.IsSubmitted == false && c.Survey.Status == SurveyStatus.Published,
                includes: c => c.Include(c => c.Survey)).ToListAsync();

            if (surveyParticipants == null)
            {
                return new List<SurveyParticipantViewModel>();
            }

            return Mapper.Map<IEnumerable<SurveyParticipantViewModel>>(surveyParticipants);
        }

        protected override Func<IQueryable<SurveyModel>, IQueryable<SurveyModel>> FullIncludeProperties
        {
            get
            {
                return x => x.Include(m => m.Questions).ThenInclude(m => m.Options).Include(m => m.Participants);
            }
        }

        public override async Task<SurveyViewModel> CreateAsync(SurveyViewModel viewModel)
        {
            var model = Mapper.Map<SurveyModel>(viewModel);

            // Audit for child
            if (model.Questions != null && model.Questions.Any())
            {
                foreach (var question in model.Questions)
                {
                    question.Audit(model.CreatedBy);

                    if (question.Options != null && question.Options.Any())
                    {
                        foreach (var option in question.Options)
                        {
                            option.Audit(model.CreatedBy);
                        }
                    }
                }
            }

            await Repository.AddAsync(model);
            await UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<SurveyViewModel>(model);
            return viewModel;
        }

        public async Task<SurveyViewModel> UpdateAsync(SurveyViewModel viewModel, string userName)
        {
            var model = await Repository.GetAsync(x => x.Id == viewModel.Id, includes: FullIncludeProperties);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {viewModel.Id} not found!");
            }

            Mapper.Map(viewModel, model);

            // Audit for child
            if (model.Questions.Any())
            {
                foreach (var question in model.Questions)
                {
                    question.Audit(userName);

                    if (question.Options.Any())
                    {
                        foreach (var option in question.Options)
                        {
                            option.Audit(userName);
                        }
                    }
                }
            }

            model.Audit(userName);
            await UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<SurveyViewModel>(model);
            return viewModel;
        }

        public async Task CloseAsync(long id, string userName)
        {
            var survey = await Repository.GetAsync(x => x.Id == id);
            if (survey == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            survey.Status = SurveyStatus.Closed;
            survey.ClosedDate = DateTime.UtcNow;
            survey.Audit(userName);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task<SurveyViewModel> GetDetailAsync(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(x => x.Id == id, includes: i => i.Include(x => x.Questions).ThenInclude(x => x.Options));
            return Mapper.Map<SurveyViewModel>(model);
        }

        public async Task<int> CountParticipantAsync(long id, bool? isSubmitted)
        {
            var query = _surveyParticipantRepository.QueryAsNoTracking(x => x.SurveyId == id);
            if (isSubmitted != null)
            {
                query = query.Where(x => x.IsSubmitted == isSubmitted);
            }
            return await query.CountAsync();
        }
    }
}