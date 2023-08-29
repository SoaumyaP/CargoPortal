namespace Groove.CSFE.Supplemental.Models
{
    public class PagedResultModel<T> : Pagination
    {
        public PagedResultModel()
        {

        }

        public PagedResultModel(int totalRecords, int skip, int take, string order, string direction, IEnumerable<T> records) =>
            (TotalRecords, Skip, Take, Order, Direction, Records) =
            (totalRecords, skip, take, order, direction, records);

        public int TotalRecords { get; set; }
        public IEnumerable<T> Records { get; set; }
    }
}
