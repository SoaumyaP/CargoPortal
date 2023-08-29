using Groove.SP.Core.Entities;
using MediatR;

namespace Groove.SP.Core.Events
{
    /// <summary>
    /// Event used when a shipment is cancelled
    /// </summary>
    public class ShipmentCancelledDomainEvent : INotification
    {
        public long ShipmentId { get; }

        public ShipmentCancelledDomainEvent(long shipmentId)
        {
            ShipmentId = shipmentId;
        }
    }
}
