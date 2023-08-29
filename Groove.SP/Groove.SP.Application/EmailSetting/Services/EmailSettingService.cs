using Groove.SP.Application.EmailSetting.Services.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.EmailSetting.Services
{
    public class EmailSettingService : IEmailSettingService
    {
        /// <summary>
        /// Check Compliance Email Setting to define recipients.
        /// </summary>
        /// <param name="emailSettings"></param>
        /// <param name="type"></param>
        /// <param name="defaultEmailSendTo"></param>
        /// <param name="defaultEmailCC"></param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetReceipientsFromBuyerCompliance(List<EmailSettingModel> emailSettings, EmailSettingType type, List<string> defaultEmailSendTo, List<string> defaultEmailCC)
        {
            const char EMAIL_SEPARATOR = ',';

            var mailToList = defaultEmailSendTo ?? new List<string>();
            var mailCCList = defaultEmailCC ?? new List<string>();

            var emailSetting = emailSettings?.FirstOrDefault(x => x.EmailType == type);
            if (emailSetting != null)
            {
                var sendToList = new List<string>();
                if (!string.IsNullOrWhiteSpace(emailSetting.SendTo))
                {
                    sendToList = emailSetting.SendTo.Split(EMAIL_SEPARATOR)
                                                    .Select(x => x.Trim())
                                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                                    .Distinct()
                                                    .ToList();
                }
                // merge into default list if DefaultSendTo is checked.
                // ortherwise, using email setting.
                mailToList = !emailSetting.DefaultSendTo ? sendToList : mailToList.Concat(sendToList).Distinct().ToList();

                if (!string.IsNullOrWhiteSpace(emailSetting.CC))
                {
                    // split cc by comma
                    var ccList = emailSetting.CC.Split(EMAIL_SEPARATOR)
                                                .Select(x => x.Trim())
                                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                                .Distinct()
                                                .ToList();
                    // then merge into default cc list
                    mailCCList = mailCCList != null ? mailCCList.Concat(ccList).Distinct().ToList() : ccList;
                }
            }

            return new Dictionary<string, List<string>>
            {
                {"sendTo", mailToList },
                {"cc", mailCCList}
            };
        }
    }
}
