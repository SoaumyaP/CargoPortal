using Groove.SP.Application.Mappers;
using Groove.SP.Application.ROLineItems.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.ROLineItems.Mappers
{
    public class ROLineItemMappingProfile : MappingProfileBase<ROLineItemModel, ROLineItemViewModel>
    {
        public ROLineItemMappingProfile()
        {
            CreateMap<ROLineItemModel, ROLineItemViewModel>().ReverseMap();
        }
    }
}
