using Groove.SP.Application.AppDocument.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.ShareDocument.Mappers;

public class ShareDocumentMappingProfile : MappingProfileBase<ShareDocumentModel, ShareDocumentViewModel>
{
    public ShareDocumentMappingProfile()
    {
        CreateMap<ShareDocumentModel, ShareDocumentViewModel>().ReverseMap();
    }
}