using System;
using System.Collections.Generic;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class DashBoardSummaryGridViewModel<T>
    {
        public DateTime QueryStartDate { get; set; }
        public DateTime QueryEndDate { get; set; }

        public IEnumerable<T> Data { get; set; }
    }
}
