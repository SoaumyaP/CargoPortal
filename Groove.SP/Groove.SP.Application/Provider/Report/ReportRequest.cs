// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportRequest.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Groove.SP.Application.Provider.Report
{
    public class ReportRequest
    {
        /// <summary>
        /// [CategoryName]/[ReportName]
        /// </summary>
        public string ReportName { get; set; }

        public ReportFormat ReportFormat { get; set; }

        public dynamic ReportParameters { get; set; }
    }
}