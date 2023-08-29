using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Common;
using FluentValidation;
using System;
using Groove.SP.Core.Models;
using System.Linq;
using AngleSharp.Text;

namespace Groove.SP.Application.CargoDetail.Validations
{
    public class CargoDetailValidation : BaseValidation<CargoDetailViewModel>
    {
        public CargoDetailValidation(bool isUpdating = false)
        {
            if (isUpdating)
            {
                ValidateUpdate();
            }
            else
            {
                ValidateAdd();
            }
        }

        private void ValidateAdd()
        {
            RuleFor(a => a.Id).NotNull();
            RuleFor(a => a.ShipmentId).NotNull();
            RuleFor(a => a.Sequence).NotNull();
            RuleFor(a => a.Unit).NotNull();
            RuleFor(a => a.Package).NotNull();
            RuleFor(a => a.Volume).NotNull();
            RuleFor(a => a.GrossWeight).NotNull();
            RuleFor(a => a.NetWeight).NotNull();
            RuleFor(a => a.OrderType)
               .Must(x => Enum.GetNames(typeof(OrderType)).Contains(x, StringComparison.InvariantCultureIgnoreCase))
               .When(x => !string.IsNullOrEmpty(x.OrderType))
               .WithMessage($"Inputted data is not valid. Available values are: {string.Join(',', Enum.GetNames(typeof(OrderType)))}.");
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.ShipmentId).NotEmpty().When(x => x.IsPropertyDirty("ShipmentId"));
            RuleFor(a => a.Sequence).NotNull().When(x => x.IsPropertyDirty("Sequence"));
            RuleFor(a => a.Unit).NotNull().When(x => x.IsPropertyDirty("Unit"));
            RuleFor(a => a.Package).NotNull().When(x => x.IsPropertyDirty("Package"));
            RuleFor(a => a.Volume).NotNull().When(x => x.IsPropertyDirty("Volume"));
            RuleFor(a => a.GrossWeight).NotNull().When(x => x.IsPropertyDirty("GrossWeight"));
            RuleFor(a => a.NetWeight).NotNull().When(x => x.IsPropertyDirty("NetWeight"));
            RuleFor(a => a.OrderType)
               .Must(x => Enum.GetNames(typeof(OrderType)).Contains(x, StringComparison.InvariantCultureIgnoreCase))
               .When(x => x.IsPropertyDirty($"{nameof(CargoDetailViewModel.OrderType)}"))
               .WithMessage($"Inputted data is not valid. Available values are: {string.Join(',', Enum.GetNames(typeof(OrderType)))}.");
        }
    }
}
