using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Common;
using FluentValidation;
using System.Linq;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class InputPOFulfillmentValidator : BaseValidation<InputPOFulfillmentViewModel>
    {
        public InputPOFulfillmentValidator()
        {
            RuleFor(m => m.Contacts).Must(m => m.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Principal) != null);
            RuleFor(m => m.Contacts).Must(m => m.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Supplier) != null).When(x => x.OrderFulfillmentPolicy != OrderFulfillmentPolicy.AllowMissingPO);
            RuleForEach(m => m.Loads).SetValidator(new POFulfillmentLoadValidator());
            RuleForEach(m => m.Itineraries).SetValidator(new POFulfillmentItineraryValidator()).When(m => m.Stage >= POFulfillmentStage.ForwarderBookingRequest);
            RuleForEach(m => m.Orders).SetValidator(x => new POFulfillmentOrderValidator(x.OrderFulfillmentPolicy));
            RuleFor(m => m.FulfilledFromPOType).IsInEnum();
        }
    }
}
