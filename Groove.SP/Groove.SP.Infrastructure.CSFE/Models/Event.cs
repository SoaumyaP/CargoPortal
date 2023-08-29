namespace Groove.SP.Infrastructure.CSFE.Models
{
    public class Event
    {
        public string ActivityCode { get; set; }

        public string ActivityType { get; set; }

        public string ActivityDescription { get; set; }

        public bool LocationRequired { get; set; }

        public bool RemarkRequired { get; set; }
    }
}
