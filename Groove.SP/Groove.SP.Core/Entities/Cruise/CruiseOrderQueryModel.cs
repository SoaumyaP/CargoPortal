using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Core.Entities.Cruise
{
    public class CruiseOrderQueryModel
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public DateTime? PODate { get; set; }
        public string POStatus { get; set; }
        public string Consignee { get; set; }
        public string Supplier { get; set; }
        //public string Milestone { get; set; }
    }
}
