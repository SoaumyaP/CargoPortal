using Groove.CSFE.Application.Common;
using Groove.CSFE.Core;

namespace Groove.CSFE.Application.Vessels.ViewModels
{
    public class VesselListViewModel
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
        public bool IsRealVessel { get; set; }
        public VesselStatus Status { get; set; }

        private string _statusName;
        public string StatusName
        {
            get
            {
                return string.IsNullOrEmpty(_statusName) ? EnumHelper<VesselStatus>.GetDisplayValue(this.Status) : _statusName;
            }
            set => _statusName = value;
        }
    }
}
