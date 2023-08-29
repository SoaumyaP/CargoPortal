using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Attachment.Mappers
{
    public class AttachmentMappingProfile : MappingProfileBase<AttachmentModel, AttachmentViewModel>
    {
        public AttachmentMappingProfile()
        {
            CreateMap<AttachmentModel, AttachmentViewModel>()
                .ForMember(d => d.UploadedDateTime, x => x.MapFrom(y => y.UploadedDate))
                .ReverseMap()
                .ForMember(d => d.UploadedDate, x => x.MapFrom(y => y.UploadedDateTime))
                .ForMember(d => d.RowVersion, x => x.Ignore());
        }
    }
}
