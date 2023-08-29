using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Users.ViewModels;

namespace Groove.SP.Application.Users.Validations
{
    public class UserRequestsValidation : BaseValidation<UserRequestViewModel>
    {
        public UserRequestsValidation()
        {
            RuleFor(a => a.Name).NotEmpty();
            RuleFor(a => a.CompanyName).NotNull();
            RuleFor(a => a.RoleId).NotNull();
            RuleFor(a => a.Username).NotEmpty();
            RuleFor(a => a.Phone).Length(1, 32).When(x => !string.IsNullOrEmpty(x.Phone));
            RuleFor(a => a.Email).NotEmpty().EmailAddress();
        }
    }
}
