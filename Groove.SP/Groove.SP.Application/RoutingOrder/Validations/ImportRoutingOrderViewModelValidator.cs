using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.ROLineItems.Validations;
using Groove.SP.Application.RoutingOrder.ViewModels;
using Groove.SP.Application.RoutingOrderContact.Validations;
using Groove.SP.Application.RoutingOrderContainer.Validations;
using Groove.SP.Application.RoutingOrderInvoice.Validations;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.RoutingOrder.Validations
{
    public class ImportRoutingOrderViewModelValidator : BaseValidation<ImportRoutingOrderViewModel>
    {
        #region Variables
        private readonly List<int> validStatuses = new()
        {
            (int)RoutingOrderStatus.Active,
            (int)RoutingOrderStatus.Cancel
        };

        private readonly List<string> validIncoterms = new()
        {
            IncotermType.CFR.ToString(),
            IncotermType.CIF.ToString(),
            IncotermType.CIP.ToString(),
            IncotermType.CPT.ToString(),
            IncotermType.DAP.ToString(),
            IncotermType.DAT.ToString(),
            IncotermType.DDP.ToString(),
            IncotermType.EXW.ToString(),
            IncotermType.FAS.ToString(),
            IncotermType.FCA.ToString(),
            IncotermType.FOB.ToString()
        };

        private readonly List<string> validModeOfTransports = new()
        {
            ModeOfTransport.Sea,
            ModeOfTransport.Air
        };

        private readonly List<string> validSeaLogisticServices = new()
        {
            LogisticServiceType.InternationalPortToPort,
            LogisticServiceType.InternationalPortToDoor,
            LogisticServiceType.InternationalDoorToPort,
            LogisticServiceType.InternationalDoorToDoor
        };

        private readonly List<string> validAirLogisticServices = new()
        {
            LogisticServiceType.InternationalAirportToAirport,
            LogisticServiceType.InternationalAirportToDoor,
            LogisticServiceType.InternationalDoorToAirport,
            LogisticServiceType.InternationalDoorToDoor
        };

        private readonly List<string> validMovementTypes = new()
        {
            "CY",
            "CFS"
        };

        private readonly List<string> requiredContactRoles = new()
        {
            OrganizationRole.Shipper,
            OrganizationRole.Consignee,
            OrganizationRole.Supplier,
            OrganizationRole.DestinationAgent
        };
        #endregion

        public ImportRoutingOrderViewModelValidator()
        {
            RuleFor(x => x.RoutingOrderNumber).NotEmpty();
            RuleFor(x => x.RoutingOrderDate).NotEmpty();
            RuleFor(x => x.Status)
                .Must(status => validStatuses.Contains(status.Value))
                .When(x => x.Status.HasValue)
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", validStatuses)}.");
            RuleFor(x => x.NumberOfLineItems).NotEmpty();
            RuleFor(x => x.NumberOfLineItems)
                .Must((viewModel, numberOfLineItems) => numberOfLineItems == viewModel.LineItems.Count)
                .When(x => x.LineItems != null)
                .WithMessage($"Value should be matched with number of product line items.");
            RuleFor(x => x.Incoterm).NotEmpty();
            RuleFor(x => x.Incoterm)
                .Must(incoterm => validIncoterms.Contains(incoterm))
                .When(x => !string.IsNullOrWhiteSpace(x.Incoterm))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", validIncoterms)}.");
            RuleFor(x => x.ModeOfTransport).NotEmpty();
            RuleFor(x => x.ModeOfTransport)
                .Must(modeOfTransport => validModeOfTransports.Contains(modeOfTransport))
                .When(x => !string.IsNullOrWhiteSpace(x.ModeOfTransport))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", validModeOfTransports)}.");
            RuleFor(x => x.LogisticsService)
                .Must((viewModel, logisticsService) => IsValidLogisticService(logisticsService, viewModel))
                .When(x => !string.IsNullOrWhiteSpace(x.LogisticsService))
                .WithMessage(x => $"Invalid value found. Supported values are: {StringifyValidLogisticService(x.ModeOfTransport)}.");
            RuleFor(x => x.MovementType).NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.ModeOfTransport) && x.ModeOfTransport.ToLower() == ModeOfTransport.Sea.ToLower());
            RuleFor(x => x.MovementType)
                .Must(movementType => validMovementTypes.Contains(movementType))
                .When(x => !string.IsNullOrWhiteSpace(x.MovementType))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", validMovementTypes)}.");

            RuleFor(x => x.ShipFrom).NotEmpty();
            RuleFor(x => x.ShipTo).NotEmpty();

            RuleFor(x => x.IsContainDangerousGoods)
                .Must(isContainDangerousGoods => Enum.IsDefined(typeof(BooleanOption), isContainDangerousGoods))
                .When(x => !string.IsNullOrWhiteSpace(x.IsContainDangerousGoods))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", typeof(BooleanOption).GetEnumNames())}.");
            RuleFor(x => x.IsBatteryOrChemical)
                .Must(isBatteryOrChemical => Enum.IsDefined(typeof(BooleanOption), isBatteryOrChemical))
                .When(x => !string.IsNullOrWhiteSpace(x.IsBatteryOrChemical))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", typeof(BooleanOption).GetEnumNames())}.");
            RuleFor(x => x.IsCIQOrFumigation)
                .Must(isCIQOrFumigation => Enum.IsDefined(typeof(BooleanOption), isCIQOrFumigation))
                .When(x => !string.IsNullOrWhiteSpace(x.IsCIQOrFumigation))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", typeof(BooleanOption).GetEnumNames())}.");
            RuleFor(x => x.IsExportLicence)
                .Must(isExportLicence => Enum.IsDefined(typeof(BooleanOption), isExportLicence))
                .When(x => !string.IsNullOrWhiteSpace(x.IsExportLicence))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", typeof(BooleanOption).GetEnumNames())}.");

            RuleFor(x => x.CreatedBy).NotEmpty();
            RuleFor(x => x.CreatedBy).MaximumLength(256);

            RuleFor(x => x.LineItems).NotNull();
            RuleFor(x => x.LineItems).Must(lineItems => lineItems.Count > 0).WithMessage($"'{nameof(ImportRoutingOrderViewModel.LineItems)}' must not be empty.");
            RuleForEach(x => x.LineItems).SetValidator(new ImportROLineItemViewModelValidator());

            RuleForEach(x => x.Invoices).SetValidator(new ImportRoutingOrderInvoiceViewModelValidator());

            RuleFor(x => x.Containers).NotNull();
            RuleFor(x => x.Containers).Must(containers => containers.Count > 0).WithMessage($"'{nameof(ImportRoutingOrderViewModel.Containers)}' must not be empty.");
            RuleForEach(x => x.Containers).SetValidator(x => new ImportRoutingOrderContainerViewModelValidator(x.ModeOfTransport, x.MovementType));

            RuleFor(x => x.Contacts).NotNull();
            RuleFor(x => x.Contacts).Must(contacts => contacts.Count > 0).WithMessage($"'{nameof(ImportRoutingOrderViewModel.Contacts)}' must not be empty.");
            RuleForEach(x => x.Contacts).SetValidator(x => new ImportRoutingOrderContactViewModelValidator());

            foreach (var role in requiredContactRoles)
            {
                RuleFor(x => x.Contacts)
                    .Must(contacts => contacts.Any(x => x.OrganizationRole == role))
                    .WithMessage($"Missing {role} contact. Required contacts are {string.Join(", ", requiredContactRoles)}");
            }
        }

        private bool IsValidLogisticService(string logisticService, ImportRoutingOrderViewModel viewModel)
        {
            if (!string.IsNullOrWhiteSpace(logisticService))
            {
                if (viewModel.ModeOfTransport?.ToLower() == ModeOfTransport.Sea.ToLower())
                {
                    return validSeaLogisticServices.Contains(logisticService);
                }
                else if (viewModel.ModeOfTransport?.ToLower() == ModeOfTransport.Air.ToLower())
                {
                    return validAirLogisticServices.Contains(logisticService);
                }
            }
            return true;
        }

        private string StringifyValidLogisticService(string modeOfTransport)
        {
            if (!string.IsNullOrWhiteSpace(modeOfTransport))
            {
                if (modeOfTransport.ToLower() == ModeOfTransport.Sea.ToLower())
                {
                    return string.Join(", ", validSeaLogisticServices);
                }
                else if (modeOfTransport.ToLower() == ModeOfTransport.Air.ToLower())
                {
                    return string.Join(", ", validAirLogisticServices);
                }
            }
            return string.Empty;
        }
    }
}