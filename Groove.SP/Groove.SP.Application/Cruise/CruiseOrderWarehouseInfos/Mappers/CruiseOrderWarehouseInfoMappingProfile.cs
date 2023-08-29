using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities.Cruise;

namespace Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.Mappers
{
    public class CruiseOrderWarehouseInfoMappingProfile : MappingProfileBase<CruiseOrderWarehouseInfoModel, CruiseOrderWarehouseInfoViewModel>
    {
        public CruiseOrderWarehouseInfoMappingProfile()
        {
            CreateMap<CruiseOrderWarehouseInfoModel, CruiseOrderWarehouseInfoViewModel>()
                .ReverseMap()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
