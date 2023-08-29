namespace Groove.SP.Application.ApplicationBackgroundJob.ViewModels
{
    public class SendEmailViewModel
    {
        public string MailTo { get; set; }

        public string MailSubject { get; set; }

        public string EmailBody { get; set; }

        public string JobDescription { get; set; }
    }
}
