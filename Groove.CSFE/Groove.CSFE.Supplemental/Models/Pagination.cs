namespace Groove.CSFE.Supplemental.Models
{
    public abstract class Pagination
    {
        public int Skip { get; set; } = 1;
        public int Take { get; set; } = 10;
        public string Order { get; set; } = "";
        public string Direction { get; set; } = "asc";
    }
}
