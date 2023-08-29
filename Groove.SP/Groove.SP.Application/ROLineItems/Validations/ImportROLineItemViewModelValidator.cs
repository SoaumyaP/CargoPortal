using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.ROLineItems.ViewModels;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Application.ROLineItems.Validations
{
    public class ImportROLineItemViewModelValidator : BaseValidation<ImportROLineItemViewModel>
    {
        private readonly List<string> validCommodityTypes = new()
        {
            Commodity.GeneralGoods,
            Commodity.Garments,
            Commodity.Accessories,
            Commodity.Toys,
            Commodity.PlasticGoods,
            Commodity.Household,
            Commodity.Textiles,
            Commodity.Hardware,
            Commodity.Stationery,
            Commodity.Houseware,
            Commodity.Kitchenware,
            Commodity.Footwear,
            Commodity.Furniture,
            Commodity.Electionics,
            Commodity.ElectricalGoods,
            Commodity.NonperishableGroceries
        };
        public ImportROLineItemViewModelValidator()
        {
            RuleFor(x => x.ItemNo).NotEmpty();
            RuleFor(x => x.DescriptionOfGoods).NotEmpty();
            RuleFor(x => x.OrderedUnitQty).NotEmpty();
            RuleFor(x => x.OrderedUnitQty).GreaterThanOrEqualTo(0);
            RuleFor(x => x.UnitUOM).NotEmpty();
            RuleFor(x => x.UnitUOM)
                .Must(unitUOM => Enum.IsDefined(typeof(UnitUOMType), unitUOM))
                .When(x => !string.IsNullOrWhiteSpace(x.UnitUOM))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", typeof(UnitUOMType).GetEnumNames())}.");
            RuleFor(x => x.BookedPackage).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PackageUOM)
                .Must(packageUOM => Enum.IsDefined(typeof(PackageUOMType), packageUOM))
                .When(x => !string.IsNullOrWhiteSpace(x.PackageUOM))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", typeof(PackageUOMType).GetEnumNames())}.");
            RuleFor(x => x.Commodity)
                .Must(commodity => validCommodityTypes.Contains(commodity))
                .When(x => !string.IsNullOrWhiteSpace(x.Commodity))
                .WithMessage($"Invalid value found. Supported values are: {string.Join(", ", validCommodityTypes)}.");
        }
    }
}
