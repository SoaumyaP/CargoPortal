using System;
namespace Groove.SP.Core.Entities
{
    public class GlobalIdActivityModel: Entity
    {
        public long Id { get; set; }

        public string GlobalId { get; set; }

        public long ActivityId { get; set; }

        public GlobalIdModel ReferenceEntity { get; set; }

        public ActivityModel Activity { get; set; }

        public string Location { get; set; }

        public string Remark { get; set; }

        public DateTime ActivityDate { get; set; }
    }
}
