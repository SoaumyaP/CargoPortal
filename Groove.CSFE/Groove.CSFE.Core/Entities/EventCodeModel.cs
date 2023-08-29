namespace Groove.CSFE.Core.Entities
{
    public class EventCodeModel : Entity
    {
        public string ActivityCode { get; set; }

        public string ActivityTypeCode { get; set; }

        public virtual EventTypeModel ActivityType { get; set; }

        public string ActivityDescription { get; set; }

        public bool LocationRequired { get; set; }

        public bool RemarkRequired { get; set; }

        public long SortSequence { get; set; }

        public EventCodeStatus Status { get; set; }
    }
}
