
namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class POLineItemArticleMasterViewModel
    {
        public long Id { get; set; }
        public string ItemNo { get; set; }
        public string StyleNo { get; set; }
        public string ColourCode { get; set; }
        public string Size { get; set; }
        public string StyleName { get; set; }
        public string ColourName { get; set; }
        public int? InnerQuantity { get; set; }
        public int? OuterQuantity { get; set; }
        public decimal? OuterDepth { get; set; }
        public decimal? OuterHeight { get; set; }
        public decimal? OuterWidth { get; set; }
        public decimal? OuterGrossWeight { get; set; }
    }
}
