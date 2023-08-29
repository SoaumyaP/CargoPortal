using FluentValidation;
using Groove.SP.Application.Container.ViewModels;

namespace Groove.SP.Application.Container.Validations
{
    public class UpdateContainerViaUIValidator : AbstractValidator<UpdateContainerViaUIViewModel>
    {
        public UpdateContainerViaUIValidator()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.ContainerNo).NotEmpty();
        }
    }
}
