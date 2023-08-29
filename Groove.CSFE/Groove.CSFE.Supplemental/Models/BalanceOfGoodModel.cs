namespace Groove.CSFE.Supplemental.Models
{
    public class BalanceOfGoodModel
    {
        public int PrincipleId { get; set; }
        public string PrincipleCode { get; set; }
        public string PrincipleName { get; set; }
        public int ArticleId { get; set; }
        public string ArticleCode { get; set; }
        public string ArticleName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set;  }
        public string WarehouseName { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal ShippedQuantity { get; set; }
        public decimal AdjustQuantity { get; set; }
        public decimal DamageQuantity { get; set; }
    }
}
