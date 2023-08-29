using System;

namespace Groove.SP.Core.Entities
{
    public class MasterDialogQueryModel
    {
        public long Id { set; get; }

        public DateTime CreatedDateOnly { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Owner { get; set; }

        public string Category { get; set; }

        public string DisplayOn { get; set; }

        public string FilterCriteria { get; set; }

        public string FilterValue { get; set; }

        public string Message { get; set; }
    }
}