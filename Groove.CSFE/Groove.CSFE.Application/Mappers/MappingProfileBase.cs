using AutoMapper;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Utilities;
using Groove.CSFE.Core;

namespace Groove.CSFE.Application.Mappers
{
    public abstract class MappingProfileBase<TModel, TViewModel> : Profile
        where TModel : Entity
        where TViewModel : ViewModelBase<TModel>
    {
        public MappingProfileBase()
        {
            
        }
    }
}
