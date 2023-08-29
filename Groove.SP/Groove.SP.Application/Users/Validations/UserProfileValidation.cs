using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Users.ViewModels;

namespace Groove.SP.Application.Users.Validations
{
    public class UserProfileValidation : BaseValidation<UserProfileViewModel>
    {
        public UserProfileValidation()
        {
            RuleFor(a => a.Name).NotEmpty();
            RuleFor(a => a.Phone).Length(1, 32).When(x => !string.IsNullOrEmpty(x.Phone));
            RuleFor(a => a.Email).NotEmpty().EmailAddress();
            RuleFor(a => a.CompanyName).NotEmpty().When(x => !x.IsInternal);
        }
    }
}
