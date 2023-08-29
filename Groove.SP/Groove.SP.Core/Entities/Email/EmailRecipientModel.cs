namespace Groove.SP.Core.Entities.Email
{
    public class EmailRecipientModel : Entity
    {
        public long Id { get; set; }
        /// <summary>
        /// Template name, has to match to name of email template file
        /// </summary>
        public string TemplateName { get; set; }
        /// <summary>
        /// To. Multiple values separated by ';'
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// CC. Multiple values separated by ';'
        /// </summary>
        public string CC { get; set; }
        /// <summary>
        /// BCC. Multiple values separated by ';'
        /// </summary>
        public string BCC { get; set; }

    }
}
