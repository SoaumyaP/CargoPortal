using System;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;

namespace Groove.SP.Application.PurchaseOrders.Validations
{
    public class UpdatePOViewModelValidator : BaseValidation<UpdatePOViewModel>
    {
        public UpdatePOViewModelValidator(ICSFEApiClient csfeApiClient,
            IRepository<PurchaseOrderModel> poRepository)
        {
            var ports = csfeApiClient.GetAllPortsAsync().Result;
            var currencies = csfeApiClient.GetAllCurrenciesAsync().Result;
            var carries = csfeApiClient.GetAllCarriesAsync().Result;

            RuleFor(a => a.PONumber).NotEmpty().When(x => x.IsPropertyDirty("PONumber"));
            RuleFor(a => a.POIssueDate).NotNull().When(x => x.IsPropertyDirty("POIssueDate"));
            RuleFor(a => a.CreatedDate).NotEmpty().When(x => x.IsPropertyDirty("CreatedDate"));
            RuleFor(a => a.Contacts).NotNull().When(x => x.IsPropertyDirty("Contacts"));
            RuleForEach(a => a.Contacts).SetValidator(new CreateOrUpdatePOContactViewModelValidator(csfeApiClient)).When(x => x.IsPropertyDirty("Contacts"));

            RuleFor(a => a.LineItems).NotNull().When(x => x.IsPropertyDirty("LineItems"));
            RuleFor(a => a.LineItems)
                .Must(x => !x.GroupBy(item => new { item.LineOrder, item.ScheduleLineNo }).Any(g => g.Count() > 1))
                .WithMessage("Duplicate Line Order.")
                .When(x => x.IsPropertyDirty("LineItems"));
            RuleForEach(a => a.LineItems).SetValidator(new POLineItemViewModelValidator(csfeApiClient)).When(x => x.IsPropertyDirty("LineItems"));

            RuleFor(a => a.LineItems)
               .Must(x => !x.Where(item => !string.IsNullOrWhiteSpace(item.POLineKey))
                        .GroupBy(item => item.POLineKey)
                        .Any(g => g.Count() > 1))
               .WithMessage("Duplicate 'POLineKey'.")
               .When(x => x.IsPropertyDirty("LineItems")); ;

            RuleFor(x => x.NumberOfLineItems)
               .NotEmpty()
               .When(x => x.IsPropertyDirty("NumberOfLineItems"));
            RuleFor(x => x)
                .Must(x => x.NumberOfLineItems == x.LineItems?.Count())
                .WithMessage("The number of item in 'LineItems' must be equal to 'NumberOfLineItems'.")
                .When(x => x.IsPropertyDirty("LineItems") || x.IsPropertyDirty("NumberOfLineItems"));

            RuleFor(a => a.ModeOfTransport)
                .Must(s => Enum.TryParse(s.Trim(), true, out ModeOfTransportType result))
                .WithMessage("Inputted data is not existing in system.")
                .When(s => !string.IsNullOrWhiteSpace(s.ModeOfTransport));

            RuleFor(a => a.Incoterm)
                .Must(s => Enum.TryParse(s.Trim(), true, out IncotermType result))
                .WithMessage("Inputted data is not existing in system.")
                .When(s => !string.IsNullOrWhiteSpace(s.Incoterm));

            #region Blanket/ Allocated Relationship rules

            RuleFor(a => a.BlanketPOKey)
                .NotEmpty()
                .When(s => s.POType == POType.Allocated);

            RuleFor(a => a.BlanketPOKey)
                .Empty()
                .When(s => !string.IsNullOrWhiteSpace(s.POType?.ToString()) && s.POType != POType.Allocated);

            RuleFor(a => a.BlanketPOKey)
                .MustAsync(async (blanketPOKey, cancellation) => (await poRepository.AnyAsync(p => p.POKey == blanketPOKey && p.POType != POType.Allocated)))
                .WithMessage("Inputted data is not existing in system.")
                .When(s => s.IsPropertyDirty("BlanketPOKey"));

            #endregion

            RuleFor(a => a.GatewayCode)
                .Must(x => ports.Any(r => r.AirportCode.Equals(x, StringComparison.OrdinalIgnoreCase) ||
                                          r.SeaportCode.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Inputted data is not existing in system.")
                .When(x => !string.IsNullOrWhiteSpace(x.GatewayCode));

            RuleFor(a => a.CarrierCode)
                .Must(x => carries.Any(r => r.CarrierCode.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Inputted data is not existing in system.")
                .When(x => !string.IsNullOrWhiteSpace(x.CarrierCode));

            RuleFor(a => a.PaymentCurrencyCode)
                .Must(x => currencies.Any(r => r.Code.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Inputted data is not existing in system.")
                .When(x => !string.IsNullOrWhiteSpace(x.PaymentCurrencyCode));

            RuleFor(a => a.Contacts)
               .Must(x => x.Any(r => OrganizationRole.Principal.Equals(r.OrganizationRole, StringComparison.OrdinalIgnoreCase)))
               .WithMessage("PO Contact is missing Principal Organization.")
               .When(x => x.IsPropertyDirty("Contacts"));

            RuleFor(a => a.Contacts)
               .Must(x => x.Any(r => OrganizationRole.Supplier.Equals(r.OrganizationRole, StringComparison.OrdinalIgnoreCase)))
               .WithMessage("PO Contact is missing Supplier Organization.")
               .When(x => x.IsPropertyDirty("Contacts"));

            RuleFor(a => a.ContainerType).IsInEnum()
             .WithMessage("Inputted data is not existing in system.");

            RuleFor(a => a.POType).IsInEnum()
                .WithMessage($"Inputted data is not valid. Available values are: {string.Join(',', Enum.GetNames(typeof(POType)))}.");

            RuleFor(a => a.CargoReadyDate)
                .Must((viewModel, property) => viewModel.CargoReadyDate <= viewModel.ExpectedShipDate)
                .When(x => !string.IsNullOrWhiteSpace(x.CargoReadyDate?.ToString()) && !string.IsNullOrWhiteSpace(x.ExpectedShipDate?.ToString()))
                .WithMessage($"Cargo Ready Date must not later than the Expected Ship Date");

            RuleFor(a => a.ExpectedShipDate)
                .Must((viewModel, property) => viewModel.ExpectedShipDate >= viewModel.CargoReadyDate)
                .When(x => !string.IsNullOrWhiteSpace(x.ExpectedShipDate?.ToString()) && !string.IsNullOrWhiteSpace(x.CargoReadyDate?.ToString()))
                .WithMessage($"Expected Ship Date must not earlier than the Cargo Ready Date");

            RuleFor(a => a.ExpectedShipDate)
                .Must((viewModel, property) => viewModel.ExpectedShipDate <= viewModel.ExpectedDeliveryDate)
                .When(x => !string.IsNullOrWhiteSpace(x.ExpectedShipDate?.ToString()) && !string.IsNullOrWhiteSpace(x.ExpectedDeliveryDate?.ToString()))
                .WithMessage($"Expected Ship Date must not later than the Expected Delivery Date");

            RuleFor(a => a.ExpectedDeliveryDate)
                .Must((viewModel, property) => viewModel.ExpectedDeliveryDate >= viewModel.ExpectedShipDate)
                .When(x => !string.IsNullOrWhiteSpace(x.ExpectedDeliveryDate?.ToString()) && !string.IsNullOrWhiteSpace(x.ExpectedShipDate?.ToString()))
                .WithMessage($"Expected Delivery Date must not earlier than the Expected Ship Date");
        }
    }
}
