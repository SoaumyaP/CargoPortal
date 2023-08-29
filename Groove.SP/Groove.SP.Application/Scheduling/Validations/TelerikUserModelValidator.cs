using FluentValidation;
using Groove.SP.Application.Scheduling.ViewModels;

namespace Groove.SP.Application.Scheduling.Validations
{
    public class TelerikUserModelValidator : AbstractValidator<TelerikUserModel>
    {
        public TelerikUserModelValidator()
        {
            RuleFor(a => a.Username).NotEmpty().NotNull();
            RuleFor(a => a.Password).NotEmpty().NotNull();
            RuleFor(a => a.Email).NotEmpty().NotNull().EmailAddress();
            RuleFor(a => a.FirstName).NotEmpty().NotNull();
            RuleFor(a => a.LastName).NotEmpty().NotNull();
        }
    }
}
