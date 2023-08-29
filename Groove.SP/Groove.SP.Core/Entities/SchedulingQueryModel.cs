using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class SchedulingQueryModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string ReportGroup { get; set; }

        public string ReportName { get; set; }

        public SchedulingStatus Status { get; set; }

        public string StatusName { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? NextOccurrence { get; set; }

        public string TelerikSchedulingId { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
