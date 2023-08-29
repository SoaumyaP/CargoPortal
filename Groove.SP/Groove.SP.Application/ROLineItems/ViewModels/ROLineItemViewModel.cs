using Groove.SP.Application.Common;
using Groove.SP.Application.RoutingOrder.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.ROLineItems.ViewModels
{
    public class ROLineItemViewModel : ViewModelBase<ROLineItemModel>
    {
        public long Id { get; set; }
        public long RoutingOrderId { get; set; }
        public string PONumber { get; set; }
        public string ItemNo { get; set; }
        public string DescriptionOfGoods { get; set; }
        public string ChineseDescription { get; set; }
        public int OrderedUnitQty { get; set; }
        public UnitUOMType UnitUOM { get; set; }
        public int? BookedPackage { get; set; }
        public PackageUOMType? PackageUOM { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? Volume { get; set; }
        public string HsCode { get; set; }
        public string Commodity { get; set; }
        public string ShippingMarks { get; set; }

        public virtual RoutingOrderViewModel RoutingOrder { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
