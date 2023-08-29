namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class StatisticsPOViewModel
    {
        public int TotalPO { get; set; }

    }
    public class StatisticsPOManagedToDateViewModel
    {
        public long NumberOfPO { get; set; }
        public decimal CBM { get; set; }
        public long Units { get; set; }
        public decimal FOBPrice { get; set; }

    }
}