using Groove.SP.Application.ArticleMaster.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.ArticleMaster.Mappers;
public class ArticleMasterMappingProfile : MappingProfileBase<ArticleMasterModel, ArticleMasterViewModel>
{
    public ArticleMasterMappingProfile()
    {
        CreateMap<ArticleMasterModel, ArticleMasterViewModel>()
            .ReverseMap()
            .ForMember(src => src.RowVersion, opt => opt.Ignore())
            .ForMember(src => src.Id, opt => opt.Ignore())
            .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
    }
}