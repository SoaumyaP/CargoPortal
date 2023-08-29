using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class ImportDataProgressModel : Entity
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTime? EndDate { get; set; }

        public string Result { get; set; }

        public string Log { get; set; }

        public ImportDataProgressStatus Status { get; set; }

        public int TotalSteps { get; set; }

        public int CompletedSteps { get; set; }
    }
}
