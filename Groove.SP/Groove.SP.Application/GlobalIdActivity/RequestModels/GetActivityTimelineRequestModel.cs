using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Application.GlobalIdActivity.RequestModels
{
    public class GetActivityTimelineRequestModel
    {
        public long EntityId { get; set; }
        public string EntityType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FilterBy { get; set; }
        public string FilterValue { get; set; }
    }
}