using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Application.WarehouseAssignments.ViewModels;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.WarehouseAssignments.Mappers;

public class WarehouseAssignmentMappingProfile : MappingProfileBase<WarehouseAssignmentModel, WarehouseAssignmentViewModel>
{
    public WarehouseAssignmentMappingProfile()
    {
        CreateMap<WarehouseAssignmentModel, WarehouseAssignmentViewModel>().ReverseMap();
    }
}