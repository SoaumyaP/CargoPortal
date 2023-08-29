using System;
using System.Collections.Generic;

namespace Groove.SP.Application.Scheduling.ViewModels
{
    /// <summary>
    /// Model of Telerik report
    /// </summary>
    public class TelerikReportModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string CategoryId { get; set; }
    }

    /// <summary>
    /// Result model of Telerik reports
    /// </summary>
    public class TelerikReportResultModel
    {
        public IEnumerable<TelerikReportModel> Data { get; set; }
        public long Total { get; set; }
        public object AggregateResults { get; set; }
        public object Errors { get; set; }
    }
}
