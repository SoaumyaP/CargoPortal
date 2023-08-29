namespace Groove.SP.Application.ApplicationBackgroundJob
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Groove.SP.Application.Interfaces.Repositories;
    using Groove.SP.Application.Provider.EmailSender;
    using Groove.SP.Core.Models;
    using Microsoft.Extensions.Options;
    using RazorLight;

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
    public class SendMailBackgroundJobs
    {
        private readonly IEmailSender _emailSender;
        private readonly IRazorLightEngine _razorLight;
        private readonly IEmailRecipientRepository _emailRecipientRepository;


        public SendMailBackgroundJobs(
            IOptions<AppConfig> appConfig,
            IEmailSender emailSender,
            IRazorLightEngine razorLight,
            IEmailRecipientRepository emailRecipientRepository)
        {
            _emailSender = emailSender;
            _razorLight = razorLight;
            _emailRecipientRepository = emailRecipientRepository;
        }


        /// <summary>
        /// To email to recipients that are configured on database on table email.EmailRecipients.
        /// </summary>
        /// <remarks>
        /// Please ensure configuration available on database, else no email sent out.
        /// </remarks>
        /// <typeparam name="T">T dynamic type</typeparam>
        /// <param name="jobDescription">Description of job which is shown in Hangfire dashboard</param>
        /// <param name="templateName">Template name. It must exist on database</param>
        /// <param name="model">Data of template</param>
        /// <param name="mailSubject">Email subject</param>
        /// <param name="recipientRoleMapping">Role mapping. It is dictionary of ('role key', 'email addressed')</param>
        /// <param name="spEmailAttachments">Attachments</param>
        /// <returns></returns>
        [DisplayName("Send Mail: {0}")]
        public async Task SendMailAsync<T>(string jobDescription, string templateName, T model, string mailSubject, Dictionary<string, string> recipientRoleMapping, params SPEmailAttachment[] spEmailAttachments)
        {
            var attachments = new List<Attachment>();
            if (spEmailAttachments != null && spEmailAttachments.Any())
            {
                foreach (var emailAttachment in spEmailAttachments)
                {
                    var attachment = new Attachment(new MemoryStream(emailAttachment.AttachmentContent), emailAttachment.AttachmentName);
                    attachments.Add(attachment);
                }
            }
            string emailBody = await _razorLight.CompileRenderAsync(templateName, model);
            var recipients = this._emailRecipientRepository.QueryAsNoTracking(x => x.TemplateName == templateName).FirstOrDefault();

            // In case there is not configuration, background job will be done, no email sent out
            if (recipients != null)
            {
                var to = recipients.To;
                var cc = recipients.CC;
                var bcc = recipients.BCC;

                // To replace role parameter by correct email addresses
                if (recipientRoleMapping != null)
                {
                    foreach (var item in recipientRoleMapping.Keys)
                    {
                        if (!string.IsNullOrEmpty(to))
                        {
                            to = to.Replace(item, recipientRoleMapping[item]);
                        }
                        if (!string.IsNullOrEmpty(cc))
                        {
                            cc = cc.Replace(item, recipientRoleMapping[item]);
                        }
                        if (!string.IsNullOrEmpty(bcc))
                        {
                            bcc = bcc.Replace(item, recipientRoleMapping[item]);
                        }
                    }
                }

                // To replace all comma by semi-colon
                to = to.Replace(',', ';').Replace(" ", string.Empty);
                cc = cc.Replace(',', ';').Replace(" ", string.Empty);
                bcc = bcc.Replace(',', ';').Replace(" ", string.Empty);

                _emailSender.SendMail(mail_to: to, mail_subject: mailSubject, mail_body: emailBody, mail_cc: cc, mail_bcc: bcc, attachments: attachments.ToArray());
            }
        }



        [DisplayName("Send Mail: {0}")]
        public async Task SendMailAsync<T>(string jobDescription, string templateName, T model, string mailTo, string mailSubject, params SPEmailAttachment[] spEmailAttachments)
        {
            var attachments = new List<Attachment>();
            if (spEmailAttachments != null && spEmailAttachments.Any())
            {
                foreach (var emailAttachment in spEmailAttachments)
                {
                    var attachment = new Attachment(new MemoryStream(emailAttachment.AttachmentContent), emailAttachment.AttachmentName);
                    attachments.Add(attachment);
                }
            }
            string emailBody = await _razorLight.CompileRenderAsync(templateName, model);
            _emailSender.SendEmail(mailTo, mailSubject, emailBody, attachments.ToArray());
        }

        [DisplayName("Send Mail: {0}")]
        public async Task SendMailWithCCAsync<T>(string jobDescription, string templateName, T model, string mailTo, List<string> mailCC, string mailSubject, params SPEmailAttachment[] spEmailAttachments)
        {
            var attachments = new List<Attachment>();
            if (spEmailAttachments != null && spEmailAttachments.Any())
            {
                foreach (var emailAttachment in spEmailAttachments)
                {
                    var attachment = new Attachment(new MemoryStream(emailAttachment.AttachmentContent), emailAttachment.AttachmentName);
                    attachments.Add(attachment);
                }
            }

            string emailBody = await _razorLight.CompileRenderAsync(templateName, model);
            _emailSender.SendEmail(mailTo, mailSubject, emailBody, mailCC, attachments.ToArray());
        }

        [DisplayName("Send Mail: {0}")]
        public async Task SendMailWithCCAsync<T>(string jobDescription, string templateName, T model, string mailTo, string mailCC, string mailSubject, params SPEmailAttachment[] spEmailAttachments)
        {
            var attachments = new List<Attachment>();
            if (spEmailAttachments != null && spEmailAttachments.Any())
            {
                foreach (var emailAttachment in spEmailAttachments)
                {
                    var attachment = new Attachment(new MemoryStream(emailAttachment.AttachmentContent), emailAttachment.AttachmentName);
                    attachments.Add(attachment);
                }
            }
            string emailBody = await _razorLight.CompileRenderAsync(templateName, model);
            _emailSender.SendEmail(mailTo, mailSubject, emailBody, mailCC, attachments.ToArray());
        }

        [DisplayName("Send Mail: {0}")]
        public async Task SendMailWithBCCAsync<T>(string jobDescription, string templateName, T model, string mailTo, string mailCC, string mailBCC, string mailSubject, params SPEmailAttachment[] spEmailAttachments)
        {
            var attachments = new List<Attachment>();
            if (spEmailAttachments != null && spEmailAttachments.Any())
            {
                foreach (var emailAttachment in spEmailAttachments)
                {
                    var attachment = new Attachment(new MemoryStream(emailAttachment.AttachmentContent), emailAttachment.AttachmentName);
                    attachments.Add(attachment);
                }
            }
            string emailBody = await _razorLight.CompileRenderAsync(templateName, model);

            var to = mailTo ?? "";
            var cc = mailCC ?? "";
            var bcc = mailBCC ?? "";

            // To replace all comma by semi-colon
            to = to.Replace(',', ';').Replace(" ", string.Empty);
            cc = cc.Replace(',', ';').Replace(" ", string.Empty);
            bcc = bcc.Replace(',', ';').Replace(" ", string.Empty);

            // Send email
            _emailSender.SendMail(mail_to: to, mail_subject: mailSubject, mail_body: emailBody, mail_cc: cc, mail_bcc: bcc, attachments: attachments.ToArray());

        }

        [DisplayName("Send Mail: {0}")]
        public async Task SendMailWithBodyAsync(string jobDescription, string emailBody, string mailTo, string mailSubject, params Attachment[] attachments)
        {
            _emailSender.SendEmail(mailTo, mailSubject, emailBody, attachments);
        }

        public class SPEmailAttachment
        {
            public byte[] AttachmentContent { get; set; }
            public string AttachmentName { get; set; }
        }
    }
}