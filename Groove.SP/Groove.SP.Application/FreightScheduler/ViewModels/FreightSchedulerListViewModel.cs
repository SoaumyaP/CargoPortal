using System;

namespace Groove.SP.Application.FreightScheduler.ViewModels
{
    public class FreightSchedulerListViewModel
    {
        public long Id { get; set; }
        public string VesselName { get; set; }
        public string Voyage { get; set; }
        public string FlightNumber { get; set; }
        public string VesselFlight { get; set; }
        public string MAWB { get; set; }
        public string CarrierName { get; set; }
        public DateTime ETDDate { get; set; }
        public DateTime ETADate { get; set; }
    }
}
