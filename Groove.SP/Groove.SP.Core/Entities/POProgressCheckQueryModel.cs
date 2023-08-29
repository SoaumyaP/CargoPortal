using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Core.Entities
{
    public class POProgressCheckQueryModel
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public bool ProductionStarted { set; get; }
        public bool QCRequired { set; get; }
        public bool ShortShip { set; get; }
        public bool SplitShipment { set; get; }
        public DateTime? ProposeDate { set; get; }
        public string Remark { set; get; }
    }
}
