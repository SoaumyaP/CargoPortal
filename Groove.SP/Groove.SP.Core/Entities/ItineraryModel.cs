using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ItineraryModel : Entity
    {
        public long Id { get; set; }

        public long? ScheduleId { set; get; }

        public int Sequence { get; set; }

        public string ModeOfTransport { get; set; }

        public string CarrierName { get; set; }

        public string SCAC { get; set; }

        public string AirlineCode { get; set; }

        public string VesselFlight { get; set; }

        public string VesselName { get; set; }

        public string Voyage { get; set; }

        public string FlightNumber { get; set; }

        public string LoadingPort { get; set; }

        public DateTime ETDDate { get; set; }

        public DateTime ETADate { get; set; }

        public string DischargePort { get; set; }

        public string RoadFreightRef { get; set; }

        public string Status { get; set; }

        public bool IsImportFromApi { get; set; }

        public DateTime? CYOpenDate { set; get; }

        public DateTime? CYClosingDate { set; get; }

        public virtual FreightSchedulerModel FreightScheduler { set; get; }

        public virtual ICollection<ConsignmentItineraryModel> ConsignmentItineraries { get; set; }

        public virtual ICollection<ContainerItineraryModel> ContainerItineraries { get; set; }

        public virtual ICollection<BillOfLadingItineraryModel> BillOfLadingItineraries { get; set; }

        public virtual ICollection<MasterBillOfLadingItineraryModel> MasterBillOfLadingItineraries { get; set; }
    }
}
