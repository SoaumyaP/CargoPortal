using System.Collections.Generic;

namespace Groove.CSFE.Core.Entities
{
    public class EventTypeModel : Entity
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public int EventLevel { get; set; }
        public string LevelDescription { get; set; }

        public virtual ICollection<EventCodeModel> EventCodes { get; set; }
    }
}
