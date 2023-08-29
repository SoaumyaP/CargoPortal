using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillment.ViewModels;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class ImportPOFulfillmentCargoReceiveViewModelValidator : BaseValidation<ImportPOFulfillmentCargoReceiveViewModel>
    {
        public ImportPOFulfillmentCargoReceiveViewModelValidator()
        {
            RuleFor(m => m.BookingNo).NotEmpty();
            RuleFor(m => m.PlantNo).NotEmpty();
            RuleForEach(m => m.CustomerPO).SetValidator(new ImportPOFulfillmentCargoReceiveItemViewModelValidator());
        }
    }
}