using FluentValidation;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Infrastructure.CSFE;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class EdiSonUpdateConfirmPOFFValidator : AbstractValidator<EdiSonUpdateConfirmPOFFViewModel>
    {
        public EdiSonUpdateConfirmPOFFValidator(ICSFEApiClient csfeApiClient)
        {
            RuleFor(a => a.BookingReferenceNo).NotEmpty();
            RuleFor(a => a.CYEmptyPickupTerminalCode).MaximumLength(10);
            RuleFor(a => a.CYEmptyPickupTerminalDescription).MaximumLength(512);
            RuleFor(a => a.CFSWarehouseCode).MaximumLength(10);
            RuleFor(a => a.CFSWarehouseDescription).MaximumLength(512);
        }
    }
}