using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;
using Groove.SP.Application.PurchaseOrderContact.ViewModels;

namespace Groove.SP.Application.PurchaseOrderContact.Mappers{

    public class PurchaseOrderContactMappingProfile : MappingProfileBase<PurchaseOrderContactModel, PurchaseOrderContactViewModel>
    {
        public PurchaseOrderContactMappingProfile()
        {
            CreateMap<PurchaseOrderContactModel, PurchaseOrderContactViewModel>().ReverseMap();
        }
    }
}
