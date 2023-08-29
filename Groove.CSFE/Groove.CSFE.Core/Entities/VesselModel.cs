using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Core.Entities
{
    public class VesselModel : Entity
    {
        public long Id { set; get; }
        public string Code { set; get; }
        public string Name { set; get; }
        public VesselStatus Status { get; set; }
        public bool IsRealVessel { get; set; }

    }
}
