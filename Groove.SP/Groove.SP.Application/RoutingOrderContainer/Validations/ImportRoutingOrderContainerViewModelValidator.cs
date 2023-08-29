using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.RoutingOrderContainer.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Groove.SP.Application.RoutingOrderContainer.Validations
{
    public class ImportRoutingOrderContainerViewModelValidator : BaseValidation<ImportRoutingOrderContainerViewModel>
    {
        private readonly List<string> validContainerTypes = Enum.GetValues(typeof(EquipmentType)).Cast<EquipmentType>()
            .Select(x => x.GetAttributeValue<DisplayAttribute, string>(y => y.ShortName))
            .ToList();

        public ImportRoutingOrderContainerViewModelValidator(string modeOfTransport, string movementType)
        {
            RuleFor(x => x.ContainerType).NotEmpty();
            RuleFor(x => x.ContainerType)
                .Must(containerType => containerType == EquipmentType.LCL.ToString())
                .When(x => modeOfTransport == ModeOfTransport.Air || movementType == "CFS")
                .WithMessage($"Invalid value found. Supported value is: {EquipmentType.LCL.ToString()}.");
            RuleFor(x => x.ContainerType)
                .Must(containerType => validContainerTypes.Contains(containerType))
                .When(x => movementType == "CY")
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", validContainerTypes)}.");

            RuleFor(x => x.Quantity).NotEmpty().When(x => movementType == "CY");
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Volume).NotEmpty().When(x => modeOfTransport == ModeOfTransport.Air || movementType == "CFS");
        }
    }
}
