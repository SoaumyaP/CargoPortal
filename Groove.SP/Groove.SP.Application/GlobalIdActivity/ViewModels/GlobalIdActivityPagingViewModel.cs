using System;
using System.Collections.Generic;

namespace Groove.SP.Application.GlobalIdActivity.ViewModels
{
    public class GlobalIdActivityPagingViewModel
    {
        public IList<GlobalIdActivityViewModel> GlobalIdActivities { get; set; }

        public int TotalRecord { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalPage {
            get {
                return (int)Math.Ceiling((double)(TotalRecord / (double)PageSize));
            }
        }
    }
}