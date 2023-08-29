using Groove.SP.Application.BuyerApproval.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BuyerApproval.Mappers
{
    public class ConsolidationMappingProfile : MappingProfileBase<BuyerApprovalModel, BuyerApprovalViewModel>
    {
        public ConsolidationMappingProfile()
        {
            CreateMap<BuyerApprovalModel, BuyerApprovalViewModel>()
                .ForMember(src => src.Requestor, opt => opt.Ignore())
                .ForMember(src => src.RequestByOrganization, opt => opt.Ignore())
                .ForMember(src => src.Severity, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(src => src.RowVersion, opt => opt.Ignore());
        }
    }
}
