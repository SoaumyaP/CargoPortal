using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class POFulfillmentOrderViewModel : IHasViewSetting
    {
        public long Id { get; set; }

        public long PurchaseOrderId { get; set; }

        public long POLineItemId { get; set; }

        public string CustomerPONumber { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public int OrderedUnitQty { get; set; }

        public int FulfillmentUnitQty { get; set; }

        public int BalanceUnitQty { get; set; }

        public int LoadedQty { get; set; }

        public int OpenQty { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public UnitUOMType UnitUOM { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PackageUOMType PackageUOM { get; set; }

        public string Commodity { get; set; }

        public POFulfillmentOrderStatus Status { get; set; }

        public string HsCode { get; set; }

        public string CountryCodeOfOrigin { get; set; }

        public int? BookedPackage { get; set; }

        public decimal? Volume { get; set; }

        public decimal? GrossWeight { get; set; }

        public decimal? NetWeight { get; set; }

        public int? InnerQuantity { get; set; }
        public int? OuterQuantity { get; set; }

        public string ShippingMarks { get; set; }
        public string DescriptionOfGoods { get; set; }
        public string ChineseDescription { get; set; }

        //To check if user add duplicate ProductCode on CustomerPO tab, only take latest row has updated
        public bool IsLatestUpdated { get; set; }

        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.FREIGHTBOOKING_DETAIL_CUSTOMER_POS;

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }

    }
}
