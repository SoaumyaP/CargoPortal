namespace Groove.CSFE.Supplemental.Models
{
    public class BalanceOfGoodSearchModel : Pagination
    {
        public string? Principle { get; set; }
        public string? Article { get; set; }
        public string? Warehouse { get; set; }
        public string? Location { get; set; }
        public string? Keyword { get; set; }
        public IEnumerable<long>? AccessiblePrinciples { get; set; }
    }
}
