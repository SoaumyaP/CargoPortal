using Groove.SP.Application.Mappers;
using Groove.SP.Application.POFulfillmentCargoReceive.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.POFulfillmentCargoReceive.Mappers
{
    public class POFulfillmentCargoReceiveMappingProfile : MappingProfileBase<POFulfillmentCargoReceiveModel, POFulfillmentCargoReceiveViewModel>
    {
        public POFulfillmentCargoReceiveMappingProfile()
        {
            CreateMap<POFulfillmentCargoReceiveModel, POFulfillmentCargoReceiveModel>().ReverseMap();
        }
    }
}
