using System;

namespace Groove.SP.Application.GlobalIdActivity.RequestModels
{
    public class GlobalIdActivityRequestModel
    {
        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public bool Ascending { get; set; }

        public DateTime FilterEventDate { get; set; }
    }
}