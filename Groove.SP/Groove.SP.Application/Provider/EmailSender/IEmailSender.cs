using System.Collections.Generic;

namespace Groove.SP.Application.Provider.EmailSender
{
    public interface IEmailSender
    {

        bool SendEmail(string mail_to, string mail_subject, string mail_body, params System.Net.Mail.Attachment[] attachments);

        bool SendEmail(IEnumerable<string> mail_to, string mail_subject, string mail_body, params System.Net.Mail.Attachment[] attachments);

        bool SendEmail(string mail_to, string mail_subject, string mail_body, string mail_cc, params System.Net.Mail.Attachment[] attachments);

        bool SendEmail(string mail_to, string mail_subject, string mail_body, IEnumerable<string> mail_cc = null, params System.Net.Mail.Attachment[] attachments);

        bool SendEmail(IEnumerable<string> mail_to, string mail_subject, string mail_body, string mail_cc = null, params System.Net.Mail.Attachment[] attachments);

        bool SendEmail(IEnumerable<string> mail_to, string mail_subject, string mail_body, IEnumerable<string> mail_cc, params System.Net.Mail.Attachment[] attachments);

        bool SendMail(string mail_to, string mail_subject, string mail_body, string mail_cc = null, string mail_bcc = null, params System.Net.Mail.Attachment[] attachments);

    }
}
