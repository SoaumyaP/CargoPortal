using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Core.Entities
{
    public class VesselArrivalQueryModel
    {
        public long Id { get; set; }
        public string PONumber { set; get; }
        public string CarrierName { set; get; }
        public string VesselVoyage { set; get; }
        public string LoadingPort { set; get; }
        public DateTime ETDDate { set; get; }
        public string DischargePort { set; get; }
        public DateTime ETADate { set; get; }
    }
}
