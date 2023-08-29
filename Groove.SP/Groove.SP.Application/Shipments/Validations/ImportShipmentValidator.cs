using FluentValidation;
using Groove.SP.Application.CargoDetail.Validations;
using Groove.SP.Application.Container.Validations;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Itinerary.Validations;
using Groove.SP.Application.ShipmentContact.Validations;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using System;
using System.Linq;

namespace Groove.SP.Application.Shipments.Validations;

public class ImportShipmentValidator : AbstractValidator<ImportShipmentViewModel>
{
    protected readonly ICSFEApiClient _csfeApiClient;
    protected readonly IShipmentRepository _shipmentRepository;
    protected readonly IContractMasterRepository _contractMasterRepository;

    protected readonly string[] ValidModeOfTransport = {
        ModeOfTransport.Sea,
        ModeOfTransport.Air,
        ModeOfTransport.Road,
        ModeOfTransport.Courier,
        ModeOfTransport.Railway
    };

    protected readonly string[] ValidMovement = {
        Movement.CYSlashCY,
        Movement.CYSlashCFS,
        Movement.CFSSlashCY,
        Movement.CFSSlashCFS
    };

    protected readonly string[] ValidHouseBillType = {
        BillOfLadingType.HBL,
        BillOfLadingType.FCR,
        BillOfLadingType.SeawayBill,
        BillOfLadingType.TelexRelease
    };

    protected readonly string[] ValidIncoterm = Enum.GetNames(typeof(IncotermType));

    protected readonly string[] ValidLogisticServiceType =
    {
        LogisticServiceType.InternationalPortToPort,
        LogisticServiceType.InternationalPortToDoor,
        LogisticServiceType.InternationalDoorToPort,
        LogisticServiceType.InternationalDoorToDoor,
        LogisticServiceType.InternationalAirportToAirport,
        LogisticServiceType.InternationalAirportToDoor,
        LogisticServiceType.InternationalDoorToAirport
    };

    protected readonly string[] ValidLogisticServiceTypeForAir =
    {
        LogisticServiceType.InternationalDoorToDoor,
        LogisticServiceType.InternationalAirportToAirport,
        LogisticServiceType.InternationalAirportToDoor,
        LogisticServiceType.InternationalDoorToAirport
    };
    protected readonly string[] ValidLogisticServiceTypeForSea =
    {
        LogisticServiceType.InternationalPortToPort,
        LogisticServiceType.InternationalPortToDoor,
        LogisticServiceType.InternationalDoorToPort,
        LogisticServiceType.InternationalDoorToDoor
    };


    public ImportShipmentValidator(ICSFEApiClient csfeApiClient, IShipmentRepository shipmentRepository, IContractMasterRepository contractMasterRepository, ImportShipmentStatus status)
    {
        _csfeApiClient = csfeApiClient;
        _shipmentRepository = shipmentRepository;
        _contractMasterRepository = contractMasterRepository;

        var current = DateTime.Today;

        if (status == ImportShipmentStatus.N)
        {
            ImportRule();
        }

        if (status == ImportShipmentStatus.U)
        {
            UpdateRule();
        }

        if (status != ImportShipmentStatus.D)
        {
            // Check valid contract no.
            RuleFor(x => x.CarrierContractNo)
                .MustAsync(async (number, cancellation) => await _contractMasterRepository.AnyAsync(
                    x => x.CarrierContractNo == number
                    && x.Status == ContractMasterStatus.Active
                    && x.ValidFrom <= current
                    && x.ValidTo >= current))
                .WithMessage($"'{nameof(ImportShipmentViewModel.CarrierContractNo)}' is not existing in system.")
                .When(x => !string.IsNullOrEmpty(x.CarrierContractNo));

            RuleFor(x => x.ModeOfTransport)
                   .Must(x => ValidModeOfTransport.Contains(x, StringComparer.CurrentCultureIgnoreCase))
                   .WithMessage($"'{nameof(ImportShipmentViewModel.ModeOfTransport)}' must be '{string.Join(", ", ValidModeOfTransport)}'.")
                   .When(x => !string.IsNullOrEmpty(x.ModeOfTransport));

            RuleFor(x => x.Movement)
                .Must(x => ValidMovement.Contains(x, StringComparer.CurrentCultureIgnoreCase))
                .WithMessage($"'{nameof(ImportShipmentViewModel.Movement)}' must be '{string.Join(", ", ValidMovement)}'.")
                .When(x => !string.IsNullOrEmpty(x.Movement));

            RuleFor(x => x.HouseBillType)
                .Must(x => ValidHouseBillType.Contains(x, StringComparer.CurrentCultureIgnoreCase))
                .WithMessage($"'{nameof(ImportShipmentViewModel.HouseBillType)}' must be '{string.Join(", ", ValidHouseBillType)}'.")
                .When(x => !string.IsNullOrEmpty(x.HouseBillType));

            RuleFor(x => x.Incoterm)
                .Must(x => ValidIncoterm.Contains(x, StringComparer.CurrentCultureIgnoreCase))
                .WithMessage($"'{nameof(ImportShipmentViewModel.Incoterm)}' must be '{string.Join(", ", ValidIncoterm)}'.")
                .When(x => !string.IsNullOrEmpty(x.Incoterm));

            RuleForEach(a => a.Contacts).SetValidator(a => new ImportShipmentContactValidator());
            RuleForEach(a => a.Containers).SetValidator(a => new ImportShipmentContainerValidator(IsLCL(a.Movement)));
            RuleForEach(a => a.CargoDetails).SetValidator(a => new ImportShipmentCargoDetailValidator(csfeApiClient));
            RuleForEach(a => a.Itineraries).SetValidator(a => new ImportItineraryValidator(csfeApiClient));
        }
        else
        {
            DeleteRule();
        }
    }

    protected void ImportRule()
    {
        RuleFor(x => x.ShipmentNo).NotEmpty();
        RuleFor(x => x.ModeOfTransport).NotEmpty();
        RuleFor(x => x.Movement).NotEmpty().When(x => !IsAirShipment(x.ModeOfTransport));

        RuleFor(x => x.ShipFrom).NotEmpty();
        RuleFor(x => x.ShipFromETDDate).NotEmpty();
        RuleFor(x => x.ShipTo).NotEmpty();
        RuleFor(x => x.ShipToETADate).NotEmpty();

        RuleFor(x => x.ServiceType).NotEmpty();
        RuleFor(x => x.ServiceType)
            .Must(x => ValidLogisticServiceTypeForSea.Contains(x, StringComparer.CurrentCultureIgnoreCase))
            .WithMessage($"'{nameof(ImportShipmentViewModel.ServiceType)}' must be '{string.Join(", ", ValidLogisticServiceTypeForSea)}'.")
            .When(x => !IsAirShipment(x.ModeOfTransport));
        RuleFor(x => x.ServiceType)
           .Must(x => ValidLogisticServiceTypeForAir.Contains(x, StringComparer.CurrentCultureIgnoreCase))
           .WithMessage($"'{nameof(ImportShipmentViewModel.ServiceType)}' must be '{string.Join(", ", ValidLogisticServiceTypeForAir)}'.")
           .When(x => IsAirShipment(x.ModeOfTransport));

        // Unique number for each Principal
        RuleFor(x => x.ShipmentNo)
            .MustAsync(async (shipmentNo, cancelation) => !await _shipmentRepository.AnyAsync(x => x.ShipmentNo == shipmentNo))
            .WithMessage($"'{nameof(ImportShipmentViewModel.ShipmentNo)}' cannot be duplicated.")
            .When(x => !string.IsNullOrWhiteSpace(x.ShipmentNo));
    }

    protected void UpdateRule()
    {
        RuleFor(x => x.ShipmentNo).NotEmpty();
        RuleFor(x => x.ModeOfTransport).NotEmpty().When(x => x.IsPropertyDirty(nameof(ImportShipmentViewModel.ModeOfTransport)));
        RuleFor(x => x.ShipFrom).NotEmpty().When(x => x.IsPropertyDirty(nameof(ImportShipmentViewModel.ShipFrom)));
        RuleFor(x => x.ShipFromETDDate).NotEmpty().When(x => x.IsPropertyDirty(nameof(ImportShipmentViewModel.ShipFromETDDate)));
        RuleFor(x => x.ShipTo).NotEmpty().When(x => x.IsPropertyDirty(nameof(ImportShipmentViewModel.ShipTo)));
        RuleFor(x => x.ShipToETADate).NotEmpty().When(x => x.IsPropertyDirty(nameof(ImportShipmentViewModel.ShipToETADate)));
        RuleFor(x => x.ServiceType).NotEmpty().When(x => x.IsPropertyDirty(nameof(ImportShipmentViewModel.ServiceType)));

    }

    protected void DeleteRule()
    {
        RuleFor(x => x.ShipmentNo).NotEmpty();
    }

    protected bool IsLCL(string movement)
    {
        if (string.IsNullOrEmpty(movement))
        {
            return false;
        }
        return movement.Equals(Movement.CFSSlashCY, StringComparison.CurrentCultureIgnoreCase)
            || movement.Equals(Movement.CFSSlashCFS, StringComparison.CurrentCultureIgnoreCase);
    }

    protected bool IsAirShipment(string modeOfTransport)
    {
        return !string.IsNullOrEmpty(modeOfTransport) && modeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase);
    }
}