using Groove.SP.Application.Common;
using Groove.SP.Application.PurchaseOrders.Validations;
using Groove.SP.Core.Entities;
using FluentValidation;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Application.ViewSetting.Interfaces;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class POLineItemViewModel : ViewModelBase<POLineItemModel>, IHasViewSetting
    {
        public long Id { get; set; }
        public string POLineKey { get; set; }
        public int LineOrder { get; set; }
        public int? OrderedUnitQty { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public UnitUOMType UnitUOM { get; set; }
        public int? BookedUnitQty { get; set; }
        public int? BalanceUnitQty { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyCode { get; set; }
        public string ProductFamily { get; set; }
        public string HSCode { get; set; }
        public string ChineseDescription { get; set; }
        public string SupplierProductCode { get; set; }
        public int? MinPackageQty { get; set; }
        public int? MinOrderQty { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageUOMType? PackageUOM { get; set; }
        public string CountryCodeOfOrigin { get; set; }
        public string Commodity { get; set; }
        public string ReferenceNumber1 { get; set; }
        public string ReferenceNumber2 { get; set; }
        public string ShippingMarks { get; set; }
        public string DescriptionOfGoods { get; set; }
        public string PackagingInstruction { get; set; }
        public string ProductRemark { get; set; }
        public string SeasonCode { get; set; }
        public string StyleNo { get; set; }
        public string ColourCode { get; set; }
        public string Size { get; set; }
        public decimal? Volume { get; set; }
        public decimal? GrossWeight { get; set; }

        [JsonIgnore]
        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.PO_DETAIL_ITEMS;
        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }

        #region TUMI customer
        // -> store data but without use at the moment
        public int? ScheduleLineNo { get; set; }
        public string InboundDelivery { get; set; }
        public string POItemReference { get; set; }
        public string ShipmentNo { get; set; }
        public string Plant { get; set; }
        public string StorageLocation { get; set; }
        public string MatGrpDe { get; set; }
        public string MaterialType { get; set; }
        public string Sku { get; set; }
        public string GridValue { get; set; }
        public string StockCategory { get; set; }
        public string HeaderText { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? NetWeight { get; set; }
        public string FactoryName { get; set; }

        #endregion TUMI customer

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
