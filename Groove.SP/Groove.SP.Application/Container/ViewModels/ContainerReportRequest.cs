using Groove.SP.Application.Provider.Report;

namespace Groove.SP.Application.Container.ViewModels
{
    public class ContainerReportRequest
    {
        public long ContainerId { get; set; }

        public ReportFormat ReportFormat { get; set; } = ReportFormat.Pdf;
    }
}
