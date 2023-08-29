using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Users.ViewModels;

namespace Groove.SP.Application.Users.Validations
{
    public class RoleValidation : BaseValidation<RoleViewModel>
    {
        public RoleValidation()
        {
            RuleFor(a => a.Description).MaximumLength(500);
        }
    }
}
