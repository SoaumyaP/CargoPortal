using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Core.Entities
{
    public class VesselQueryModel
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public bool IsRealVessel { set; get; }

        public VesselStatus Status { get; set; }
    }
}
