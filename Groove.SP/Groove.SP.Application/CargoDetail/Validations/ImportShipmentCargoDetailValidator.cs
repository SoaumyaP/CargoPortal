using FluentValidation;
using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentLoadDetails.Validations;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using System;
using System.Linq;

namespace Groove.SP.Application.CargoDetail.Validations
{
    public class ImportShipmentCargoDetailValidator : BaseValidation<ImportShipmentCargoDetailViewModel>
    {
        protected readonly ICSFEApiClient _csfeApiClient;
        public ImportShipmentCargoDetailValidator(ICSFEApiClient csfeApiClient)
        {
            _csfeApiClient = csfeApiClient;

            RuleFor(x => x.Sequence).NotEmpty();
            RuleFor(x => x.Unit).NotEmpty();

            RuleFor(x => x.UnitUOM).NotEmpty();
            RuleFor(x => x.UnitUOM)
                .Must(x => Enum.IsDefined(typeof(UnitUOMType), x))
                .WithMessage($"'{nameof(ImportShipmentCargoDetailViewModel.UnitUOM)}' must be '{ string.Join(", ", Enum.GetNames(typeof(UnitUOMType)))}'.")
                .When(x => !string.IsNullOrWhiteSpace(x.UnitUOM));

            RuleFor(x => x.GrossWeight).NotEmpty();
            RuleFor(x => x.GrossWeightUOM).NotEmpty();
            RuleFor(x => x.Package).NotEmpty();

            RuleFor(x => x.PackageUOM).NotEmpty();
            RuleFor(x => x.PackageUOM)
                .Must(x => Enum.IsDefined(typeof(PackageUOMType), x))
                .WithMessage($"'{nameof(ImportShipmentCargoDetailViewModel.PackageUOM)}' must be '{ string.Join(", ", Enum.GetNames(typeof(PackageUOMType)))}'.")
                .When(x => !string.IsNullOrWhiteSpace(x.PackageUOM));

            RuleFor(x => x.Volume).NotEmpty();
            RuleFor(x => x.VolumeUOM).NotEmpty();

            RuleFor(x => x.HSCode)
                .Must(x => CommonHelper.CheckValidHSCode(x))
                .WithMessage($"'{nameof(ImportShipmentCargoDetailViewModel.HSCode)}' is invalid. Each HS Code is separated by a comma and its length can only be: 4, 6, 8, 10 and 13.")
                .When(x => !string.IsNullOrWhiteSpace(x.HSCode));

            RuleFor(x => x.Commodity)
                .Must(x => ValidCommodity.Contains(x))
                .WithMessage($"'{nameof(ImportShipmentCargoDetailViewModel.Commodity)}' must be '{ string.Join(", ", ValidCommodity)}'.")
                .When(x => !string.IsNullOrWhiteSpace(x.Commodity));

            RuleFor(x => x.CountryOfOrigin)
                .MustAsync(async (code, cancellation) => await _csfeApiClient.GetCountryByCodeAsync(code) != null)
                .WithMessage($"'{nameof(ImportShipmentCargoDetailViewModel.CountryOfOrigin)}' is not existing in master data.")
                .When(x => !string.IsNullOrWhiteSpace(x.CountryOfOrigin));

            RuleForEach(x => x.LoadDetails).SetValidator(x => new ImportShipmentLoadDetailValidator());
        }

        protected string[] ValidCommodity =
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
    }
}
