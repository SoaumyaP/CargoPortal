using MediatR;
using System;

namespace Groove.SP.Core.Events
{
    /// <summary>
    /// Event used when an activity is changed
    /// </summary>
    public class ActivityChangedDomainEvent : INotification
    {
        public long? CurrentContainerId { get; }
        public long? CurrentShipmentId { get; }
        public long? CurrentBookingId { get; }
        public long? CurrentFreightSchedulerId { get; }
        public string PreviousActivityCode { get; }
        public string CurrentActivityCode { get; }
        public DateTime CurrentActivityDate { get; }
        public string CurrentLocation { get; }
        public string CurrentRemark { get; set; }

        public ActivityChangedDomainEvent(string previousActivityCode, 
            string currentActivityCode,
            DateTime currentActivityDate,
            long? currentContainerId = null, 
            long? currentShipmentId = null,
            long? currentBookingId = null,
            long? currentFreightSchedulerId = null,
            string currentLocation = null,
            string currentRemark = null
            )
        {
            PreviousActivityCode = previousActivityCode;
            CurrentActivityCode = currentActivityCode;
            CurrentContainerId = currentContainerId;
            CurrentShipmentId = currentShipmentId;
            CurrentBookingId = currentBookingId;
            CurrentFreightSchedulerId = currentFreightSchedulerId;
            CurrentActivityDate = currentActivityDate;
            CurrentLocation = currentLocation;
            CurrentRemark = currentRemark;
        }
    }
}
