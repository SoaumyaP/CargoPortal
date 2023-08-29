using Groove.CSFE.Application.Common;
using Groove.CSFE.Core;

namespace Groove.CSFE.Application.Carriers.ViewModels
{
    public class CarrierListViewModel
    {
        public long Id { get; set; }
        public string CarrierCodeNumber { get; set; }
        public string CarrierCode { get; set; }
        public string ModeOfTransport { get; set; }
        public string Name { get; set; }
        public int? CarrierNumber { get; set; }
        public byte Status { get; set; }

        private string _statusName;
        public string StatusName
        {
            get
            {
                return string.IsNullOrEmpty(_statusName) ? EnumHelper<CarrierStatus>.GetDisplayValue((CarrierStatus)Status) : _statusName;
            }
            set => _statusName = value;
        }
    }
}
