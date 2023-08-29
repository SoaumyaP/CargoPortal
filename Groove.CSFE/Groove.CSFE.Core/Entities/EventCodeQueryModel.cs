using System;

namespace Groove.CSFE.Core.Entities
{
    public class EventCodeQueryModel
    {
        public string ActivityCode { get; set; }

        public string ActivityTypeCode { get; set; }
        
        public string ActivityTypeDescription { get; set; }

        public string ActivityDescription { get; set; }

        public string LocationRequired { get; set; }

        public string RemarkRequired { get; set; }

        public long SortSequence { get; set; }

        public EventCodeStatus Status { get; set; }

        public string StatusName { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
