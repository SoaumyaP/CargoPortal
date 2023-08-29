using FluentValidation;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.POFulfillmentContact.Validations;
using Groove.SP.Infrastructure.CSFE;
using System.Threading.Tasks;
using System.Linq;
using Groove.SP.Core.Models;
using System;
using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Groove.SP.Application.Utilities;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class ImportBookingViewModelValidator : AbstractValidator<ImportBookingViewModel>
    {
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly List<EquipmentType> acceptableContainerTypes = new List<EquipmentType>() {
            EquipmentType.TwentyGP,
            EquipmentType.TwentyHC,
            EquipmentType.TwentyNOR,
            EquipmentType.TwentyRF,
            EquipmentType.FourtyGP,
            EquipmentType.FourtyHC,
            EquipmentType.FourtyNOR,
            EquipmentType.FourtyRF,
            EquipmentType.FourtyFiveHC
        };

        public ImportBookingViewModelValidator(ICSFEApiClient csfeApiClient, POFulfillmentStatus status)
        {
            _csfeApiClient = csfeApiClient;

            if (status == POFulfillmentStatus.Active)
            {
                ImportRule();
            }
            else
            {
                CancelRule();
            }
        }

        private void ImportRule()
        {
            #region Bookings
            RuleFor(a => a.Owner).NotEmpty();

            RuleFor(a => a.Status).NotEmpty();

            RuleFor(a => a.ExpectedShipDate).NotEmpty();
            RuleFor(a => a)
                .Must(a => (a.ExpectedShipDate >= a.CargoReadyDate || a.CargoReadyDate == null) && (a.ExpectedShipDate <= a.ExpectedDeliveryDate || a.ExpectedDeliveryDate == null))
                .WithMessage($"'{nameof(ImportBookingViewModel.ExpectedShipDate)}' must be greater than {nameof(ImportBookingViewModel.CargoReadyDate)} and less than {nameof(ImportBookingViewModel.ExpectedDeliveryDate)}")
                .WithName(nameof(ImportBookingViewModel.ExpectedShipDate))
                .When(a => a.ExpectedShipDate != null);

            RuleFor(a => a.Incoterm).NotEmpty();

            RuleFor(a => a.ModeOfTransport).NotEmpty();

            RuleFor(a => a.PreferredCarrier)
                .MustAsync(async (preferredCarrier, cancellation) => await IsExistCarrierByCodeAsync(preferredCarrier))
                .WithMessage($"'{nameof(ImportBookingViewModel.PreferredCarrier)}' is not existing on master data, must be carrier code.")
                .When(a => !string.IsNullOrEmpty(a.PreferredCarrier));

            RuleFor(a => a.LogisticsService).NotEmpty();

            RuleFor(a => a.MovementType).NotEmpty();

            RuleFor(a => a.ShipFrom).NotEmpty();
            RuleFor(a => a.ShipFrom)
                .MustAsync(async (shipFrom, cancellation) => (await _csfeApiClient.GetLocationByCodeAsync(shipFrom)) != null)
                .WithMessage($"'{nameof(ImportBookingViewModel.ShipFrom)}' is not existing on master data, must be port code.")
                .When(x => !string.IsNullOrWhiteSpace(x.ShipFrom));

            RuleFor(a => a.ShipTo).NotEmpty();
            RuleFor(a => a.ShipTo)
                .MustAsync(async (shipTo, cancellation) => (await _csfeApiClient.GetLocationByCodeAsync(shipTo)) != null)
                .WithMessage($"'{nameof(ImportBookingViewModel.ShipTo)}' is not existing on master data, must be port code.")
                .When(x => !string.IsNullOrWhiteSpace(x.ShipTo));

            RuleFor(a => a.DeliveryPort).NotEmpty();
            RuleFor(a => a.DeliveryPort)
                .MustAsync(async (deliveryPort, cancellation) => (await _csfeApiClient.GetLocationByCodeAsync(deliveryPort)) != null)
                .WithMessage($"'{nameof(ImportBookingViewModel.DeliveryPort)}' is not existing on master data, must be port code.")
                .When(x => !string.IsNullOrWhiteSpace(x.DeliveryPort));

            RuleFor(a => a.ReceiptPort).NotEmpty();
            RuleFor(a => a.ReceiptPort)
                .MustAsync(async (receiptPort, cancellation) => (await _csfeApiClient.GetLocationByCodeAsync(receiptPort)) != null)
                .WithMessage($"'{nameof(ImportBookingViewModel.ReceiptPort)}' is not existing on master data, must be port code.")
                .When(x => !string.IsNullOrWhiteSpace(x.ReceiptPort));

            RuleFor(a => a.BookingDate).NotEmpty();

            RuleFor(a => a.CreatedBy).NotEmpty();

            RuleFor(a => a.CreatedDate).NotEmpty();
            #endregion Booking

            RuleFor(a => a.SONumber).MaximumLength(25);

            #region Contacts
            RuleFor(a => a.Contacts).NotNull();

            RuleFor(a => a.Contacts)
               .Must(x => x.Any(r => OrganizationRole.Shipper.Equals(r.OrganizationRole, StringComparison.OrdinalIgnoreCase)))
               .WithMessage("Contact is missing Shipper Organization.")
               .When(x => x.Contacts != null);

            RuleFor(a => a.Contacts)
               .Must(x => x.Any(r => OrganizationRole.Consignee.Equals(r.OrganizationRole, StringComparison.OrdinalIgnoreCase)))
               .WithMessage("Contact is missing Consignee Organization.")
               .When(x => x.Contacts != null);

            RuleFor(a => a.Contacts)
               .Must(x => x.Any(r => OrganizationRole.Supplier.Equals(r.OrganizationRole, StringComparison.OrdinalIgnoreCase)))
               .WithMessage("Contact is missing Supplier Organization.")
               .When(x => x.Contacts != null);

            RuleFor(a => a.Contacts)
               .Must(x => x.Any(r => OrganizationRole.Principal.Equals(r.OrganizationRole, StringComparison.OrdinalIgnoreCase) && r.OrganizationCode != "0"))
               .WithMessage("Contact is missing Principal Organization.")
               .When(x => x.Contacts != null);

            RuleFor(a => a.Contacts)
               .Must(x => x.Any(r => OrganizationRole.OriginAgent.Equals(r.OrganizationRole, StringComparison.OrdinalIgnoreCase)))
               .WithMessage("Contact is missing Origin Agent Organization.")
               .When(x => x.Contacts != null);

            RuleFor(a => a.Contacts)
               .Must(x => x.Any(r => OrganizationRole.DestinationAgent.Equals(r.OrganizationRole, StringComparison.OrdinalIgnoreCase)))
               .WithMessage("Contact is missing Destination Agent Organization.")
               .When(x => x.Contacts != null);

            RuleForEach(a => a.Contacts).SetValidator(new ImportBookingContactValidator(_csfeApiClient));
            #endregion Contacts

            #region Loads
            RuleFor(a => a.Loads).NotNull();

            RuleFor(a => a.Loads)
                .Must(a => a.Count() > 0)
                .WithMessage($"{nameof(ImportBookingViewModel.Loads)}' must not be empty.")
                .When(a => a.Loads != null);

            RuleFor(a => a.Loads)
                .Must(a => !a.Any(x => x.EquipmentType != EquipmentType.LCL))
                .WithMessage("EquipmentType must be LCL for CFS Booking.")
                .When(a => a.MovementType == ImportMovementType.CFS);

            RuleFor(a => a.Loads)
                .Must(a => !a.Any(x => !acceptableContainerTypes.Contains(x.EquipmentType)))
                .WithMessage($"ContainerType must be '{string.Join(", ", acceptableContainerTypes.Select(x => x.GetAttributeValue<DisplayAttribute, string>(y => y.ShortName)))}' for CY Booking.")
                .When(a => a.MovementType == ImportMovementType.CY);

            RuleForEach(a => a.Loads).SetValidator(new ImportBookingLoadValidator());
            #endregion Loads

            #region Customer POs
            RuleFor(a => a.CustomerPOs).NotNull();

            RuleFor(a => a.CustomerPOs)
                .Must(a => a.Count() > 0)
                .WithMessage($"{nameof(ImportBookingViewModel.CustomerPOs)}' must not be empty.")
                .When(a => a.CustomerPOs != null);

            RuleForEach(a => a.CustomerPOs)
                .SetValidator(a => new ImportBookingOrderValidator(
                    a.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal)?.OrganizationCode,
                    a.ShipFrom,
                    a.ShipTo,
                    _csfeApiClient));
            #endregion Customer POs

            #region Legs
            RuleFor(a => a.Legs).NotNull();

            RuleFor(a => a.Legs)
                .Must(a => a.Count() > 0)
                .WithMessage($"{nameof(ImportBookingViewModel.Legs)}' must not be empty.")
                .When(a => a.Legs != null);

            RuleForEach(a => a.Legs).SetValidator(new ImportBookingLegValidator(_csfeApiClient));
            #endregion Legs
        }

        private void CancelRule()
        {
            RuleFor(a => a.SONumber).MaximumLength(25);
        }

        private async Task<bool> IsExistCarrierByCodeAsync(string carrierCode)
        {
            var carrier = await _csfeApiClient.GetCarrierByCodeAsync(carrierCode);
            return carrier != null;
        }
    }
}