using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillment.ViewModels;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class ImportPOFulfillmentCargoReceiveItemViewModelValidator : BaseValidation<ImportPOFulfillmentCargoReceiveItemViewModel>
    {
        public ImportPOFulfillmentCargoReceiveItemViewModelValidator()
        {
            RuleFor(m => m.PONo).NotEmpty();
            RuleFor(m => m.POSeq).NotEmpty();
            RuleFor(m => m.ProductCode).NotEmpty();
            RuleFor(m => m.Quantity).NotEmpty();
            RuleFor(m => m.InDate).NotEmpty();
            RuleFor(m => m.UnitUOM).IsInEnum().When(m => m.UnitUOM.HasValue);
        }
    }
}
