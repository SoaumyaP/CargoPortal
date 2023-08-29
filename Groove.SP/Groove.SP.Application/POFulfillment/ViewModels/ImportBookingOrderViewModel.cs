using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Runtime.Serialization;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class ImportBookingOrderViewModel : ViewModelBase<POFulfillmentOrderModel>
    {
        public string CustomerPONumber { get; set; }

        public string ProductCode { get; set; }

        public int FulfillmentUnitQty { get; set; }

        public UnitUOMType UnitUOM { get; set; }

        public PackageUOMType PackageUOM { get; set; }

        public ImportCommodityType Commodity { get; set; }

        public string HsCode { get; set; }

        public string CountryCodeOfOrigin { get; set; }

        public int BookedPackage { get; set; }

        public decimal Volume { get; set; }

        public decimal GrossWeight { get; set; }

        public decimal NetWeight { get; set; }

        public string ShippingMarks { get; set; }

        public string ChineseDescription { get; set; }

        public string ColourCode { get; set; }

        public int? Height { get; set; }

        public int? Length { get; set; }

        public int? Width { get; set; }

        public string SeasonCode { get; set; }

        public string Size { get; set; }

        public string StyleNo { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();
        }
    }

    public enum ImportCommodityType
    {
        [EnumMember(Value = "General Goods")]
        GeneralGoods = 1 << 0,

        Garments = 1 << 1,

        Accessories = 1 << 2,

        Toys = 1 << 3,

        [EnumMember(Value = "Plastic Goods")]
        PlasticGoods = 1 << 4,

        Household = 1 << 5,

        Textiles = 1 << 6,

        Hardware = 1 << 7,

        Stationery = 1 << 8,

        Houseware = 1 << 9,

        Kitchenware = 1 << 10,

        Footwear = 1 << 11,

        Furniture = 1 << 12,

        Electionics = 1 << 13,

        [EnumMember(Value = "Electrical Goods")]
        ElectricalGoods = 1 << 14,

        [EnumMember(Value = "Non-perishable Groceries")]
        NonPerishableGroceries = 1 << 15
    }
}
