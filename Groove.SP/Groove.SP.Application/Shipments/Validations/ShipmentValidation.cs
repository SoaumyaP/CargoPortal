using Groove.SP.Application.Common;
using Groove.SP.Application.Shipments.ViewModels;
using FluentValidation;
using System;
using Groove.SP.Core.Models;
using System.Linq;
using AngleSharp.Text;

namespace Groove.SP.Application.Shipments.Validations
{
    public class ShipmentValidation : BaseValidation<ShipmentViewModel>
    {
        public ShipmentValidation(bool isUpdating = false)
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
            RuleFor(a => a.ShipmentNo).NotEmpty();
            RuleFor(a => a.CargoReadyDate).NotNull();
            RuleFor(a => a.BookingDate).NotNull();
            RuleFor(a => a.ShipFrom).NotEmpty();
            RuleFor(a => a.ShipFromETDDate).NotEmpty();
            RuleFor(a => a.TotalGrossWeight).NotNull();
            RuleFor(a => a.TotalNetWeight).NotNull();
            RuleFor(a => a.TotalPackage).NotNull();
            RuleFor(a => a.TotalUnit).NotNull();
            RuleFor(a => a.TotalVolume).NotNull();
            RuleFor(a => a.Status).NotNull();
            RuleFor(a => a.IsFCL).NotNull();
            RuleFor(a => a.OrderType)
                .Must(x => Enum.GetNames(typeof(OrderType)).Contains(x, StringComparison.InvariantCultureIgnoreCase))
                .When(x => !string.IsNullOrEmpty(x.OrderType))
                .WithMessage($"Inputted data is not valid. Available values are: {string.Join(',', Enum.GetNames(typeof(OrderType)))}.");
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.ShipmentNo).NotEmpty().When(x => x.IsPropertyDirty("ShipmentNo"));
            RuleFor(a => a.CargoReadyDate).NotEmpty().When(x => x.IsPropertyDirty("CargoReadyDate"));
            RuleFor(a => a.BookingDate).NotEmpty().When(x => x.IsPropertyDirty("BookingDate"));
            RuleFor(a => a.ShipFrom).NotEmpty().When(x => x.IsPropertyDirty("ShipFrom"));
            RuleFor(a => a.ShipFromETDDate).NotEmpty().When(x => x.IsPropertyDirty("ShipFromETDDate"));
            RuleFor(a => a.TotalGrossWeight).NotNull().When(x => x.IsPropertyDirty("TotalGrossWeight"));
            RuleFor(a => a.TotalNetWeight).NotNull().When(x => x.IsPropertyDirty("TotalNetWeight"));
            RuleFor(a => a.TotalPackage).NotNull().When(x => x.IsPropertyDirty("TotalPackage"));
            RuleFor(a => a.TotalUnit).NotNull().When(x => x.IsPropertyDirty("TotalUnit"));
            RuleFor(a => a.TotalVolume).NotNull().When(x => x.IsPropertyDirty("TotalVolume"));
            RuleFor(a => a.Status).NotNull().When(x => x.IsPropertyDirty("Status"));
            RuleFor(a => a.IsFCL).NotNull().When(x => x.IsPropertyDirty("IsFCL"));
            RuleFor(a => a.OrderType)
               .Must(x => Enum.GetNames(typeof(OrderType)).Contains(x, StringComparison.InvariantCultureIgnoreCase))
               .When(x => x.IsPropertyDirty($"{nameof(ShipmentViewModel.OrderType)}"))
               .WithMessage($"Inputted data is not valid. Available values are: {string.Join(',', Enum.GetNames(typeof(OrderType)))}.");
        }
    }
}
