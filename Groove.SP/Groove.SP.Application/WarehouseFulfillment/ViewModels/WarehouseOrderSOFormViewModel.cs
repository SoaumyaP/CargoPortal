using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class WarehouseOrderSOFormViewModel
    {
        public string ShippingMarks { get; set; }
        public string CustomerPONumber { get; set; }
        public string ProductCode { get; set; }
        public string SeasonCode { get; set; }
        public string StyleNo { get; set; }
        public string StyleName { get; set; }
        public string ColourCode { get; set; }
        public string ColourName { get; set; }
        public string Size { get; set; }
        public int FulfillmentUnitQty { get; set; }
        public UnitUOMType UnitUOM { get; set; }
        public int? BookedPackage { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Volume { get; set; }
    }
}
