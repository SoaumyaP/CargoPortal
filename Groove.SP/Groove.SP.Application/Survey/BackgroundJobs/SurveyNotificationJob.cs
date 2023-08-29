using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Provider.EmailSender;
using Groove.SP.Application.Survey.ViewModels;
using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using RazorLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Core.Models;
using Microsoft.Extensions.Options;

namespace Groove.SP.Application.Survey.BackgroundJobs
{
    public class SurveyNotificationJob
    {
        private readonly IRepository<SurveyParticipantModel> _surveyParticipantRepository;
        private readonly IEmailSender _emailSender;
        private readonly IRazorLightEngine _razorLight;
        private readonly INotificationService _notificationService;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly AppConfig _appConfig;

        public SurveyNotificationJob(
            IRepository<SurveyParticipantModel> surveyParticipantRepository,
            IEmailSender emailSender,
            IRazorLightEngine razorLight,
            INotificationService notificationService,
            IUserProfileRepository userProfileRepository,
            IOptions<AppConfig> appConfig
            )
        {
            _surveyParticipantRepository = surveyParticipantRepository;
            _emailSender = emailSender;
            _razorLight = razorLight;
            _notificationService = notificationService;
            _userProfileRepository = userProfileRepository;
            _appConfig = appConfig.Value;
        }

        [DisplayName("Survey#{0} send email to participants")]
        public async Task SendEmailAsync(long surveyId)
        {
            var surveyParticipantModels = await _surveyParticipantRepository.QueryAsNoTracking(
                c => c.SurveyId == surveyId && c.IsSubmitted == false, null,
                c => c.Include(c => c.Survey)
                ).ToListAsync();

            var usernames = surveyParticipantModels.Select(c => c.Username);

            var usersProfile = await _userProfileRepository.QueryAsNoTracking(c => usernames.Contains(c.Email)).ToListAsync();

            if (surveyParticipantModels.Any())
            {
                foreach (var participant in surveyParticipantModels)
                {
                    var userInfo = usersProfile.FirstOrDefault(c => c.Email == participant.Username);

                    var email = new SurveyEmailTemplateViewModel()
                    {
                        Name = userInfo?.Name,
                        Email = participant.Username.Trim(),
                        Link = $"{_appConfig.ClientUrl}/surveys/{participant.SurveyId}/question-answer?mode=submit",
                        SurveyName = participant.Survey.Name,
                        SurveyId = participant.SurveyId
                    };

                    string emailSubject = $"Shipment Portal: survey";
                    string emailBody = await _razorLight.CompileRenderAsync("SurveySubmitAnswer", email);
                    _emailSender.SendMail(email.Email, emailSubject, emailBody);
                }
            }
        }

        [DisplayName("Survey#{0} push notifcation to participants")]
        public async Task PushNotificationAsync(long surveyId)
        {
            var surveyParticipantModels = await _surveyParticipantRepository.QueryAsNoTracking(
                c => c.SurveyId == surveyId && c.IsSubmitted == false, null,
                c => c.Include(c => c.Survey)
                ).ToListAsync();

            if (surveyParticipantModels.Any())
            {
                var userProfile = await _userProfileRepository.QueryAsNoTracking(c => surveyParticipantModels.Select(c => c.Username.Trim()).Contains(c.Email.Trim())).ToListAsync();
                var userIds = userProfile.Select(c => c.Id.ToString()).ToList();
                var emails = surveyParticipantModels.Select(c => c.Username.Trim()).ToList();
                await _notificationService.PushNotificationSilentAsync(userIds, emails, new NotificationViewModel
                {
                    MessageKey = $"~notification.msg.tellYourMindWith~ <span class=\"k-link\">{surveyParticipantModels[0]?.Survey.Name}</span>",
                    ReadUrl = $"/surveys/{surveyParticipantModels[0]?.Survey.Id}/question-answer?mode=submit"
                });
            }
        }
    }
}
