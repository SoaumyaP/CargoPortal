using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Common;
using FluentValidation;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class POFulfillmentItineraryValidator : BaseValidation<POFulfillmentItineraryViewModel>
    {
        public POFulfillmentItineraryValidator()
        {
            RuleFor(a => a.ModeOfTransport).NotEmpty();
        }
    }
}
