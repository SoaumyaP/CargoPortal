using Groove.SP.Application.Mappers;
using Groove.SP.Application.Note.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Note.Mappers
{
    public class NoteMappingProfile : MappingProfileBase<NoteModel, NoteViewModel>
    {
        public NoteMappingProfile()
        {
            CreateMap<NoteModel, NoteViewModel>();

            CreateMap<CreateAndUpdateNoteViewModel, NoteModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));
        }
    }
}
