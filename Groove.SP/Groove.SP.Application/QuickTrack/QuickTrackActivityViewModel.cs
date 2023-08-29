using System;

namespace Groove.SP.Application.QuickTrack
{
    public class QuickTrackActivityViewModel
    {
        public long Id { get; set; }

        public string ActivityCode { get; set; }

        public string ActivityType { get; set; }

        public string ActivityDescription { get; set; }

        public DateTime ActivityDate { get; set; }

        public string Location { get; set; }

        public string Remark { get; set; }
    }
}
