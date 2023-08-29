using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.CargoDetail.Mappers
{
    public class CargoDetailMappingProfile : MappingProfileBase<CargoDetailModel, CargoDetailViewModel>
    {
        public CargoDetailMappingProfile()
        {
            CreateMap<CargoDetailModel, CargoDetailViewModel>()
                .ForMember(d => d.OrderType, d => d.MapFrom(s => (int)s.OrderType));

            CreateMap<CargoDetailViewModel, CargoDetailModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<ImportShipmentCargoDetailViewModel, CargoDetailModel>()
                .ForMember(d => d.ProductNumber, d => d.MapFrom(
                    src => (src.PONumber ?? string.Empty) + (string.IsNullOrWhiteSpace(src.ProductCode) ? string.Empty : AppConstant.TILDE + src.ProductCode.Trim()) + (string.IsNullOrWhiteSpace(src.LineOrder) ? string.Empty : AppConstant.TILDE + src.LineOrder.Trim())));
        }
    }
}