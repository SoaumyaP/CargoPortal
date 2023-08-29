using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Application.ShipmentLoadDetails.Validations;

public class ImportShipmentLoadDetailValidator : BaseValidation<ImportShipmentLoadDetailViewModel>
{
    public ImportShipmentLoadDetailValidator()
    {
        RuleFor(x => x.ContainerNo).NotEmpty();
        RuleFor(x => x.Sequence).NotEmpty();
        RuleFor(x => x.Package).NotEmpty();
        RuleFor(x => x.PackageUOM).NotEmpty();
        RuleFor(x => x.PackageUOM)
                .Must(x => Enum.IsDefined(typeof(PackageUOMType), x))
                .WithMessage($"{nameof(ImportShipmentLoadDetailViewModel.PackageUOM)} must be '{ string.Join(", ", Enum.GetNames(typeof(PackageUOMType)))}'.")
                .When(x => !string.IsNullOrWhiteSpace(x.PackageUOM));
        RuleFor(x => x.Unit).NotEmpty();
        RuleFor(x => x.UnitUOM).NotEmpty();
        RuleFor(x => x.UnitUOM)
                .Must(x => Enum.IsDefined(typeof(UnitUOMType), x))
                .WithMessage($"{nameof(ImportShipmentLoadDetailViewModel.UnitUOM)} must be '{ string.Join(", ", Enum.GetNames(typeof(UnitUOMType)))}'.")
                .When(x => !string.IsNullOrWhiteSpace(x.UnitUOM));
        RuleFor(x => x.Volume).NotEmpty();
        RuleFor(x => x.VolumeUOM).NotEmpty();
        RuleFor(x => x.GrossWeight).NotEmpty();
        RuleFor(x => x.GrossWeightUOM).NotEmpty();
    }
}

