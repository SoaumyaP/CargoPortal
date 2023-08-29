using System.Collections.Generic;

namespace Groove.SP.Application.Common
{
    public class ListPagingViewModel<T>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public long TotalItem { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public int PageTotal { get; set; }
        public List<T> Items { get; set; }

        public ListPagingViewModel()
        {
            Items = new List<T>();
        }
    }
}