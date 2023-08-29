using FluentValidation;
using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Common;

namespace Groove.SP.Application.BuyerCompliance.Validations
{
    public class BookingPolicyValidator : BaseValidation<BookingPolicyViewModel>
    {
        public BookingPolicyValidator()
        {
            RuleFor(x => x.ApproverUser).EmailAddress();
        }
    }
}