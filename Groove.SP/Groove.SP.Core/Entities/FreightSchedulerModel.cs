using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Core.Entities
{
    public class FreightSchedulerModel : Entity
    {
        public long Id { set; get; }
        public string ModeOfTransport { set; get; }
        public string CarrierCode { set; get; }
        public string CarrierName { set; get; }

        public string VesselName { set; get; }
        public string Voyage { set; get; }
        public string MAWB { set; get; }
        public string FlightNumber { set; get; }

        public string LocationFromCode { get; set; }
        public string LocationFromName { set; get; }

        public string LocationToCode { get; set; }
        public string LocationToName { set; get; }

        public DateTime ETDDate { set; get; }
        public DateTime ETADate { set; get; }
        public DateTime? ATDDate { set; get; }
        public DateTime? ATADate { set; get; }

        public DateTime? CYOpenDate { set; get; }
        public DateTime? CYClosingDate { set; get; }
        
        /// <summary>
        /// If the flag is YES “Allow update from external”, IGNORE block the API update.
        /// </summary>
        public bool IsAllowExternalUpdate { get; set; }

        public virtual ICollection<ItineraryModel> Itineraries { set; get; }

        public virtual ICollection<FreightSchedulerChangeLogModel> ChangeLogs { set; get; }
    }
}
