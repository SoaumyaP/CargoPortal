using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Note.ViewModels;

namespace Groove.SP.Application.Note.Validations
{
    public class CreateAndUpdateNoteViewModelValidator : BaseValidation<CreateAndUpdateNoteViewModel>
    {
        public CreateAndUpdateNoteViewModelValidator(bool isUpdating = false)
        {
            RuleFor(x => x.NoteText).NotEmpty();
            RuleFor(x => x.Category).NotEmpty();
        }
    }
}
