using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;
using Groove.SP.Application.Reports.ViewModels;

namespace Groove.SP.Application.Reports.Mappers
{
    public class ReportMappingProfile : MappingProfileBase<ReportModel, ReportListViewModel>
    {
        public ReportMappingProfile()
        {
            CreateMap<ReportModel, ReportViewModel>();
        }
    }
}
