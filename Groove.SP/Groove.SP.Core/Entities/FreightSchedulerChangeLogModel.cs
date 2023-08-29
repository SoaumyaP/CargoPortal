namespace Groove.SP.Core.Entities
{
    public class FreightSchedulerChangeLogModel : Entity
    {
        public long Id { set; get; }

        public string JsonCurrentData { get; set; }

        public string JsonNewData { get; set; }

        public long ScheduleId { get; set; }

        public virtual FreightSchedulerModel FreightScheduler { set; get; }

    }
}
