using Groove.SP.Application.IntegrationLog.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.IntegrationLog.Mappers
{
    public class IntegrationLogMappingProfile : MappingProfileBase<IntegrationLogModel, IntegrationLogViewModel>
    {
        public IntegrationLogMappingProfile()
        {
            CreateMap<IntegrationLogModel, IntegrationLogViewModel>().ReverseMap();
        }
    }
}
