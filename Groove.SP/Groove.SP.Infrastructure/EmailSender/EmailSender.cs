using System;

namespace Groove.SP.Infrastructure.EmailSender
{
    using Groove.SP.Application.Provider.EmailSender;
    using Groove.SP.Core.Data;
    using Groove.SP.Core.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;

    public class EmailSender : IEmailSender
    {
        private readonly AppConfig _appConfig;
        private readonly ILogger<EmailSender> _logger;


        public EmailSender(IOptions<AppConfig> appConfig,
            ILogger<EmailSender> logger)
        {
            _appConfig = appConfig.Value;
            _logger = logger;
        }


        public bool SendMail(string mail_to, string mail_subject, string mail_body, string mail_cc = null, string mail_bcc = null, params Attachment[] attachments)
        {
            return SendEmailToRecipients(mail_subject,
                mail_body,
                !string.IsNullOrEmpty(mail_to) ? mail_to.Split(';', StringSplitOptions.RemoveEmptyEntries) : null,
                !string.IsNullOrEmpty(mail_cc) ? mail_cc.Split(';', StringSplitOptions.RemoveEmptyEntries) : null,
                !string.IsNullOrEmpty(mail_bcc) ? mail_bcc.Split(';', StringSplitOptions.RemoveEmptyEntries) : null,
                attachments);
        }
       
        public bool SendEmail(string mail_to, string mail_subject, string mail_body, params Attachment[] attachments)
        {
            var array_mail_to = new[] { mail_to };
            return SendEmail(array_mail_to, mail_subject, mail_body, attachments);

        }

        public bool SendEmail(IEnumerable<string> mail_to, string mail_subject, string mail_body, params Attachment[] attachments)
        {
            return SendEmailWithoutCC(mail_to, mail_subject, mail_body, attachments);

        }

        public bool SendEmail(string mail_to, string mail_subject, string mail_body, string mail_cc, params Attachment[] attachments)
        {
            var array_mail_to = new[] { mail_to };
            var array_mail_cc = new[] { mail_cc };
            return SendEmail(array_mail_to, mail_subject, mail_body, array_mail_cc, attachments);

        }

        public bool SendEmail(string mail_to, string mail_subject, string mail_body, IEnumerable<string> mail_cc = null, params Attachment[] attachments)
        {
            var array_mail_to = new[] { mail_to };
            return SendEmail(array_mail_to, mail_subject, mail_body, mail_cc, attachments);

        }

        public bool SendEmail(IEnumerable<string> mail_to, string mail_subject, string mail_body, string mail_cc = null, params Attachment[] attachments)
        {
            var array_mail_cc = new[] { mail_cc };
            return SendEmail(mail_to, mail_subject, mail_body, array_mail_cc, attachments);

        }

        public bool SendEmail(IEnumerable<string> mail_to, string mail_subject, string mail_body, IEnumerable<string> mail_cc = null, params Attachment[] attachments)
        {
            return SendEmailWithCC(mail_to, mail_subject, mail_body, mail_cc, attachments);
        }       

        private bool SendEmailWithoutCC(IEnumerable<string> mail_to, string mail_subject, string mail_body, params Attachment[] attachments)
        {
            return SendEmailWithCC(mail_to, mail_subject, mail_body, null, attachments);
        }

        private bool SendEmailWithCC(IEnumerable<string> mail_to, string mail_subject, string mail_body, IEnumerable<string> mail_cc = null, params Attachment[] attachments)
        {
            return SendEmailToRecipients(mail_subject, mail_body, mail_to, mail_cc, null, attachments);
        }

        /// <summary>
        /// To sent email to provided recipients, support: to, cc, bcc and also attachments.
        /// </summary>
        /// <param name="mail_subject">Email subject</param>
        /// <param name="mail_body">Email body content</param>
        /// <param name="mail_to">List of To</param>
        /// <param name="mail_cc">List of CC</param>
        /// <param name="mail_bcc">List of BCC</param>
        /// <param name="attachments">List of Attachments</param>
        /// <returns></returns>
        private bool SendEmailToRecipients(string mail_subject, string mail_body, IEnumerable<string> mail_to, IEnumerable<string> mail_cc = null, IEnumerable<string> mail_bcc = null, params System.Net.Mail.Attachment[] attachments)
        {
            bool result;
            try
            {
                SmtpClient client = new SmtpClient(_appConfig.SmtpHost);
                client.UseDefaultCredentials = false;
                client.Port = _appConfig.SmtpPort;
                client.Credentials = new NetworkCredential(_appConfig.EmailSenderUsername, _appConfig.EmailSenderPassword);
                // for fix error cant send email each time change email, turn on "Allow less secure apps" in this link
                // https://myaccount.google.com/lesssecureapps?pli=1
                MailMessage mailMessage = new MailMessage();

                // To support Chinese displayed consistent on both web browser and Outlook application.
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                mailMessage.From = new MailAddress(_appConfig.EmailSenderAddress, _appConfig.EmailSenderName);
                var distinctEmails = new List<string>();

                if (mail_to != null && mail_to.Any())
                {
                    mail_to.Each(emailValue =>
                    {
                        // remove space, replace comma by semi-colon
                        emailValue = emailValue.Replace(" ", string.Empty);
                        emailValue = emailValue.Replace(',', ';');

                        // to generate a list of recipients
                        var listOfEmails = emailValue.Split(';', StringSplitOptions.RemoveEmptyEntries);

                        foreach (var email in listOfEmails)
                        {
                            if (!string.IsNullOrWhiteSpace(email) && !distinctEmails.Contains(email))
                            {
                                distinctEmails.Add(email);
                                mailMessage.To.Add(new MailAddress(email));
                            }
                        }

                    });
                    
                }

                distinctEmails.Clear();
                if (mail_cc != null && mail_cc.Any())
                {
                    mail_cc.Each(emailValue =>
                    {
                        // remove space, replace comma by semi-colon
                        emailValue = emailValue.Replace(" ", string.Empty);
                        emailValue = emailValue.Replace(',', ';');

                        // to generate a list of recipients
                        var listOfEmails = emailValue.Split(';', StringSplitOptions.RemoveEmptyEntries);

                        foreach (var email in listOfEmails)
                        {
                            if (!string.IsNullOrWhiteSpace(email) && !distinctEmails.Contains(email))
                            {
                                distinctEmails.Add(email);
                                mailMessage.CC.Add(new MailAddress(email));
                            }
                        }

                    });
                }

                distinctEmails.Clear();
                if (mail_bcc != null && mail_bcc.Any())
                {
                   
                    mail_bcc.Each(emailValue =>
                    {
                        // remove space, replace comma by semi-colon
                        emailValue = emailValue.Replace(" ", string.Empty);
                        emailValue = emailValue.Replace(',', ';');

                        // to generate a list of recipients
                        var listOfEmails = emailValue.Split(';', StringSplitOptions.RemoveEmptyEntries);

                        foreach (var email in listOfEmails)
                        {
                            if (!string.IsNullOrWhiteSpace(email) && !distinctEmails.Contains(email))
                            {
                                distinctEmails.Add(email);
                                mailMessage.Bcc.Add(new MailAddress(email));
                            }
                        }

                    });
                }

                mailMessage.IsBodyHtml = true;
                mailMessage.Body = mail_body;
                mailMessage.Subject = mail_subject;

                if (attachments != null && attachments.Any())
                {
                    mailMessage.Attachments.AddRange(attachments);
                }

                client.EnableSsl = true;
                client.Send(mailMessage);

                result = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send email failed", ex.Message);
                throw;
            }
            return result;
        }

    }
}
