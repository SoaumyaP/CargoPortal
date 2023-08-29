using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Core.Entities
{
    public class FreightSchedulerQueryModel
    {
        public long Id { set; get; }
        public string ModeOfTransport { set; get; }
        public string CarrierName { set; get; }
        public string VesselName { set; get; }
        public string CarrierCode { set; get; }
        public string Voyage { set; get; }
        public string MAWB { set; get; }
        public string FlightNumber { set; get; }
        public string VesselMAWB { set; get; }
        public string LocationFromName { set; get; }
        public string LocationToName { set; get; }
        public DateTime ETDDate { set; get; }
        public DateTime ETADate { set; get; }
        public DateTime? ATDDate { set; get; }
        public DateTime? ATADate { set; get; }
        public DateTime? CYOpenDate { set; get; }
        public DateTime? CYClosingDate { set; get; }
        public bool HasLinkedItineraries { get; set; }
        public string AllowUpdateFromExternal { get; set; }
        public bool IsAllowExternalUpdate { get; set; }
    }
}
