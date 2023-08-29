using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Groove.SP.Application.Container.Validations;

public class ImportShipmentContainerValidator : BaseValidation<ImportShipmentContainerViewModel>
{
    private readonly EquipmentType[] ValidEquipmentType = {
            EquipmentType.TwentyGP,
            EquipmentType.TwentyHC,
            EquipmentType.TwentyNOR,
            EquipmentType.TwentyRF,
            EquipmentType.FourtyGP,
            EquipmentType.FourtyHC,
            EquipmentType.FourtyNOR,
            EquipmentType.FourtyRF,
            EquipmentType.FourtyFiveHC,
            EquipmentType.LCL,
            EquipmentType.Truck,
            EquipmentType.Air
    };

    public ImportShipmentContainerValidator(bool isLCL)
    {
        RuleFor(x => x.ContainerNo).NotEmpty();
        RuleFor(x => x.ContainerType).NotEmpty();
        RuleFor(x => x.ContainerType)
            .Must(x => ValidEquipmentType.Contains(x))
            .WithMessage($"'{nameof(ImportShipmentContainerViewModel.ContainerType)}' must be '{string.Join(", ", ValidEquipmentType.Select(x => x.GetAttributeValue<DisplayAttribute, string>(y => y.ShortName)))}'.")
            .When(x => x.ContainerType != 0);

        if (isLCL)
        {
            RuleFor(x => x.CFSCutoffDate).NotEmpty();
        }
    }
}