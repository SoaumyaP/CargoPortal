using Groove.SP.Application.Common;
using Groove.SP.Core.Entities.Cruise;
using System;

namespace Groove.SP.Application.CruiseOrders.ViewModels
{
    public class CruiseOrderListViewModel : ViewModelBase<CruiseOrderModel>
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public DateTime? PODate { get; set; }
        public string POStatus { get; set; }
        public string Consignee { get; set; }
        public string Supplier { get; set; }
        public string Milestone { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
