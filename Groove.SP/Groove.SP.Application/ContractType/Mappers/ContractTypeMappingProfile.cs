using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;
using Groove.SP.Application.ContractType.ViewModels;

namespace Groove.SP.Application.ContractType.Mappers
{

    public class ContractTypeMappingProfile : MappingProfileBase<ContractTypeModel, ContractTypeViewModel>
    {
        public ContractTypeMappingProfile()
        {
            CreateMap<ContractTypeModel, ContractTypeViewModel>().ReverseMap();
        }
    }
}
