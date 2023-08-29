using Groove.CSFE.Application.Currencies.ViewModels;
using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.Currencies.Mappers
{
    public class CurrencyMappingProfile : MappingProfileBase<CurrencyModel, CurrencyViewModel>
    {
        public CurrencyMappingProfile()
        {
            CreateMap<CurrencyModel, CurrencyViewModel>();
        }
    }
}
