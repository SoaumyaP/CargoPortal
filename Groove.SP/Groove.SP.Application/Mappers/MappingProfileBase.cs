using AutoMapper;
using Groove.SP.Application.Common;
using Groove.SP.Core;

namespace Groove.SP.Application.Mappers
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
