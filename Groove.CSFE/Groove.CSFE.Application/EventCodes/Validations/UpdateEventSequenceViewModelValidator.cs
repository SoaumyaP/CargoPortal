using FluentValidation;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.EventCodes.ViewModels;

namespace Groove.CSFE.Application.EventCodes.Validations
{
    public class UpdateEventSequenceViewModelValidator : BaseValidation<UpdateEventSequenceViewModel>
    {
        public UpdateEventSequenceViewModelValidator()
        {
            RuleFor(x => x.SortSequence).NotEmpty();
            RuleFor(x => x.ActivityCode).NotEmpty();
        }
    }
}
