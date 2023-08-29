using Groove.SP.Core.Entities;
using MediatR;

namespace Groove.SP.Core.Events
{
    /// <summary>
    /// Event used when an activity is created
    /// </summary>
    public class ActivityCreatedDomainEvent : INotification
    {
        public long? ContainerId { get; }
        public long? ShipmentId { get; }
        public long? FreightSchedulerId { get; }
        public ActivityModel Activity { get; }
        public string MetaData { get; }

        public ActivityCreatedDomainEvent(
            ActivityModel activity, 
            long? containerId, 
            long? shipmentId, 
            long? freightSchedulerId,
            string metaData)
        {
            Activity = activity;
            ContainerId = containerId;
            ShipmentId = shipmentId;
            FreightSchedulerId = freightSchedulerId;
            MetaData = metaData;
        }
    }
}
