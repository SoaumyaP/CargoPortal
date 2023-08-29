using Groove.SP.Application.Mappers;
using Groove.SP.Application.POFulfillmentCargoReceiveItem.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.POFulfillmentCargoReceiveItem.Mappers
{
    public class POFulfillmentCargoReceiveItemMappingProfile : MappingProfileBase<POFulfillmentCargoReceiveItemModel, POFulfillmentCargoReceiveItemViewModel>
    {
        public POFulfillmentCargoReceiveItemMappingProfile()
        {
            CreateMap<POFulfillmentCargoReceiveItemModel, POFulfillmentCargoReceiveItemViewModel>().ReverseMap();
        }
    }
}
