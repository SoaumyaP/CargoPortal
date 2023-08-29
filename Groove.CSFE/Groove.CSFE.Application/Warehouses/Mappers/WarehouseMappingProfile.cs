using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Application.Warehouses.ViewModels;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.Warehouses.Mappers
{
    public class WarehouseMappingProfile : MappingProfileBase<WarehouseModel, WarehouseViewModel>
    {
        public WarehouseMappingProfile()
        {
            CreateMap<WarehouseModel, WarehouseViewModel>().ReverseMap();
        }
    }
}
