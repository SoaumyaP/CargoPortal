using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class POFulfillmentLoadDetailModel : Entity
    {
        public long Id { get; set; }

        public long POFulfillmentLoadId { get; set; }

        public POFulfillmentLoadModel PoFulfillmentLoad { get; set; }
        
        public string CustomerPONumber { get; set; }

        public string ProductCode { get; set; }
        
        /// <summary>
        /// Loaded package
        /// </summary>
        public int PackageQuantity { get; set; }

        public PackageUOMType PackageUOM { get; set; }

        public decimal? Height { get; set; }

        public decimal? Width { get; set; }

        public decimal? Length { get; set; }

        public DimensionUnitType DimensionUnit { get; set; }

        /// <summary>
        /// Loaded quantity
        /// </summary>
        public int UnitQuantity { get; set; }

        public decimal Volume { get; set; }

        public decimal GrossWeight { get; set; }

        public decimal? NetWeight { get; set; }

        public long POFulfillmentOrderId { get; set; }

        public string ShippingMarks { get; set; }

        public string PackageDescription { get; set; }

        public int? Sequence { get; set; }       
    }
}