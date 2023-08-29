namespace Groove.CSFE.Supplemental.Models
{
    public class BalanceOfGoodsTransactionModel
    {
        public int id { get; set; }
        public int principleId { get; set; }
        public string principleCode { get; set; }
        public string principleName { get; set; }
        public int articleId { get; set; }
        public string articleCode { get; set; }
        public string articleName { get; set; }
        public int? warehouseId { get; set; }
        public string warehouseCode { get; set; }
        public string warehouseName { get; set; }
        public int? locationId { get; set; }
        public string locationName { get; set; }
        public int transTypeId { get; set; }
        public string transTypeCode { get; set; }
        public string transactionBy { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal quantity { get; set; }
        public string quantityUOM { get; set; }
        public int? poId { get; set; }
        public string poNumber { get; set; }
        public int? soId { get; set; }
        public string soNumber { get; set; }
        public int? blId { get; set; }
        public string blNumber { get; set; }
        public string documentNumber { get; set; }
        public int? noOfPackage { get; set; }
        public decimal? cbm { get; set; }
        public decimal? grossWeight { get; set; }
        public string remarks { get; set; }
    }
}