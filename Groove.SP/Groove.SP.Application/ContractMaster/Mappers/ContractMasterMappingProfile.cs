using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;
using Groove.SP.Application.ContractMaster.ViewModels;

namespace Groove.SP.Application.CruiseOrders.Mappers
{

    public class ContractMasterMappingProfile : MappingProfileBase<ContractMasterModel, ContractMasterViewModel>
    {
        public ContractMasterMappingProfile()
        {
            CreateMap<CreateContractMasterViewModel, ContractMasterModel>().ReverseMap();

            CreateMap<UpdateContractMasterViewModel, ContractMasterModel>().ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
            CreateMap<ContractMasterModel, UpdateContractMasterViewModel>();

            CreateMap<ContractMasterQueryModel, ContractMasterModel>()
                .ForMember(d => d.ValidFrom, s => s.MapFrom(x => x.ValidFromDate))
                .ForMember(d => d.ValidTo, s => s.MapFrom(x => x.ValidToDate));

            CreateMap<ContractMasterModel, ContractMasterViewModel>();
        }
    }
}
