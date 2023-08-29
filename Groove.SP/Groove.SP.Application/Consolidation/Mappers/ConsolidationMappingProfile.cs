using Groove.SP.Application.Consolidation.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Consolidation.Mappers
{
    public class ConsolidationMappingProfile : MappingProfileBase<ConsolidationModel, ConsolidationViewModel>
    {
        public ConsolidationMappingProfile()
        {
            CreateMap<ConsolidationModel, ConsolidationViewModel>()
                .ForMember(src => src.RowVersion, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<ConsolidationViewModel, ConsolidationModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<InputConsolidationViewModel, ConsolidationModel>();

            CreateMap<ConsolidationModel, ConsolidationInternalViewModel>();

            CreateMap<UpdateConsolidationViewModel, ConsolidationModel>();

            CreateMap<ConsolidationQueryModel, ConsolidationListViewModel>();

        }
    }
}