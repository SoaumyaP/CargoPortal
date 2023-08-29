using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Infrastructure.CSFE;
using FluentValidation;
using System.Threading.Tasks;
using Groove.SP.Infrastructure.CSFE.Models;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.Activity.Validations
{
    public class AgentActivityCreateViewModelValidator : BaseValidation<AgentActivityCreateViewModel>
    {
        private readonly ICSFEApiClient _csfeApiClient;
        private IEnumerable<Location> locations;
        private Event relatedEvent;

        public AgentActivityCreateViewModelValidator(ICSFEApiClient csfeApiClient)
        {
            _csfeApiClient = csfeApiClient;

            RuleFor(a => a.ActivityCode)
                .NotEmpty();

            RuleFor(a => a.ActivityCode)
                .MustAsync(async (activityCode, cancellation) => await CheckActivityCode(activityCode))
                .WithMessage($"Activity code is not existing.")
                .When(a => !string.IsNullOrEmpty(a.ActivityCode));

            RuleFor(a => a.Location)
                .NotEmpty()
                .WhenAsync(async (viewModel, cancellation) => await IsLocationRequiredAsync(viewModel));

            RuleFor(a => a.Location)
                .Must(location => CheckLocationValid(location))
                .WithMessage("Location is not existing on master data, must be location code.")
                .When(
                    a => !string.IsNullOrEmpty(a.Location)
                 );

            RuleFor(a => a.Remark)
                .NotEmpty()
                .WhenAsync(async (viewModel, cancellation) => await IsRemarkRequiredAsync(viewModel));

            RuleFor(a => a.ActivityDate)
                .NotEmpty();

            RuleFor(a => a.CustomerCode)
                .NotEmpty()
                .When(a => !string.IsNullOrEmpty(a.ShipmentNo) || !string.IsNullOrEmpty(a.PurchaseOrderNo));

            RuleFor(a => a.CustomerCode)
                .MustAsync(async (customerCode, cancellation) => await _csfeApiClient.GetOrganizationsByCodeAsync(customerCode) != null)
                .WithMessage($"Customer code is not existing.")
                .When(a => !string.IsNullOrEmpty(a.CustomerCode));

            RuleFor(a => a)
                .Must(a => (!string.IsNullOrEmpty(a.ShipmentNo) && string.IsNullOrEmpty(a.PurchaseOrderNo) && string.IsNullOrEmpty(a.ContainerNo)) ||
                     (!string.IsNullOrEmpty(a.PurchaseOrderNo) && string.IsNullOrEmpty(a.ShipmentNo) && string.IsNullOrEmpty(a.ContainerNo)) ||
                     (!string.IsNullOrEmpty(a.ContainerNo) && string.IsNullOrEmpty(a.ShipmentNo) && string.IsNullOrEmpty(a.PurchaseOrderNo)))
                .WithMessage("Either ShipmentNo, PurchaseOrderNo, or ContainerNo is allowed.")
                .WithName("ShipmentNo | PurchaseOrderNo | ContainerNo")
                .When(a => !string.IsNullOrEmpty(a.ShipmentNo) || !string.IsNullOrEmpty(a.PurchaseOrderNo) || !string.IsNullOrEmpty(a.ContainerNo));

            RuleFor(a => a)
                .Must(a => !string.IsNullOrEmpty(a.ShipmentNo) || !string.IsNullOrEmpty(a.PurchaseOrderNo) || !string.IsNullOrEmpty(a.ContainerNo))
                .WithMessage("Either ShipmentNo, PurchaseOrderNo, or ContainerNo is required.")
                .WithName("ShipmentNo | PurchaseOrderNo | ContainerNo");
        }

        private async Task<bool> CheckActivityCode(string code)
        {
            if (relatedEvent == null)
            {
                relatedEvent = await _csfeApiClient.GetEventByCodeAsync(code);
            }
            return relatedEvent != null;
        }

        private bool CheckLocationValid(string location)
        {
            if (locations == null)
            {
                locations = _csfeApiClient.GetAllLocationsAsync().Result;
            }

            return locations.Any(l => l.Name == location);

        }

        private async Task<bool> IsLocationRequiredAsync(AgentActivityCreateViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.ActivityCode))
            {
                return false;
            }

            if (relatedEvent == null)
            {
                relatedEvent = await _csfeApiClient.GetEventByCodeAsync(viewModel.ActivityCode);
            }

            return relatedEvent?.LocationRequired ?? false;
        }

        private async Task<bool> IsRemarkRequiredAsync(AgentActivityCreateViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.ActivityCode))
            {
                return false;
            }

            if (relatedEvent == null)
            {
                relatedEvent = await _csfeApiClient.GetEventByCodeAsync(viewModel.ActivityCode);
            }

            return relatedEvent?.RemarkRequired ?? false;
        }
    }
}