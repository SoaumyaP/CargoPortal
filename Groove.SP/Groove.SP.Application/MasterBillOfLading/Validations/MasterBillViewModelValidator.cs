using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Infrastructure.CSFE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Groove.SP.Application.Itinerary.Validations
{
    public class MasterBillViewModelValidator<T> : BaseValidation<T> where T : MasterBillOfLadingViewModel
    {
        protected ICSFEApiClient csfeApiClient;
        protected IEnumerable<Carrier> carriers;
        protected IEnumerable<Location> locations;
        protected IEnumerable<Vessel> vessels;

        protected void RegisterBussinessValidations()
        {
            //// Must of transport not be "Multi-model"
            RuleFor(a => a.ModeOfTransport)
                .Must(a => ValidModeOfTransports.Contains(a, comparer: StringComparer.InvariantCultureIgnoreCase))
                .WithMessage($"Mode of transport is invalid. Supported values are: {string.Join(", ", ValidModeOfTransports)}.")
                .When(a => !string.IsNullOrEmpty(a.ModeOfTransport));

            // Check SCAC if "Sea"
            RuleFor(a => a)
                .Must(a => CheckCarrierCodeValid(a))
                .WithMessage("SCAC is not existing on master data, must be carrier code.")
                .When(a =>
                   !string.IsNullOrEmpty(a.SCAC) &&
                   !string.IsNullOrEmpty(a.ModeOfTransport) &&
                   a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

            //// Check AirlineCode if "Air"
            RuleFor(a => a)
                .Must(a => CheckCarrierCodeValid(a))
                .WithMessage("Airline code is not existing on master data, must be carrier code.")
                .When(
                    a => !string.IsNullOrEmpty(a.AirlineCode) &&
                    !string.IsNullOrEmpty(a.ModeOfTransport) &&
                    a.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase));

            // Check PlaceOfReceipt
            RuleFor(a => a)
                .Must(a => CheckLocationValid(a.PlaceOfReceipt))
                .WithMessage("Place Of Receipt is not existing on master data, must be location code.")
                .When(
                    a => !string.IsNullOrEmpty(a.PlaceOfReceipt)
                    && (
                        a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) ||
                        a.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase)
                    )
                    );

            // Check PortOfLoading
            RuleFor(a => a)
                .Must(a => CheckLocationValid(a.PortOfLoading))
                .WithMessage("Port Of Loading is not existing on master data, must be location code.")
                .When(
                    a => !string.IsNullOrEmpty(a.PortOfLoading)
                    && (
                        a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) ||
                        a.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase)
                    )
                    );

            // Check PortOfDischarge
            RuleFor(a => a)
                .Must(a => CheckLocationValid(a.PortOfDischarge))
                .WithMessage("Port Of Discharge is not existing on master data, must be location code.")
                .When(
                    a => !string.IsNullOrEmpty(a.PortOfDischarge)
                    && (
                        a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) ||
                        a.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase)
                    )
                    );

            // Check PlaceOfDelivery
            RuleFor(a => a)
                .Must(a => CheckLocationValid(a.PlaceOfDelivery))
                .WithMessage("Place Of Delivery is not existing on master data, must be location code.")
                .When(
                    a => !string.IsNullOrEmpty(a.PlaceOfDelivery)
                    && (
                        a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) ||
                        a.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase)
                    )
                    );
        }

        protected bool CheckCarrierCodeValid(MasterBillOfLadingViewModel masterBill)
        {
            if (carriers == null)
            {
                carriers = csfeApiClient.GetAllCarriesAsync().Result;
            }
            var result = true;


            // Not "Air", compare on SCSC and mode of transport
            if (masterBill.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase))
            {
                result = carriers.Any(c => c.CarrierCode == masterBill.SCAC
                && !string.IsNullOrEmpty(c.ModeOfTransport)
                && c.ModeOfTransport.Equals(masterBill.ModeOfTransport, StringComparison.InvariantCultureIgnoreCase));
            }

            // Air, compare on AirlineCode and mode of transport
            else if (masterBill.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
            {
                result = carriers.Any(c => c.CarrierCode == masterBill.AirlineCode
                && !string.IsNullOrEmpty(c.ModeOfTransport)
                && c.ModeOfTransport.Equals(masterBill.ModeOfTransport, StringComparison.InvariantCultureIgnoreCase));
            }

            return result;
        }

        protected bool CheckVesselNameValid(string vesselName)
        {
            if (vessels == null)
            {
                vessels = csfeApiClient.GetAllVesselsAsync().Result;
            }

            var result = vessels.Any(v => v.Name == vesselName);

            return result;
        }

        protected bool CheckLocationValid(string locationName)
        {
            if (locations == null)
            {
                locations = csfeApiClient.GetAllLocationsAsync().Result;
            }

            return locations.Any(l => l.Name == locationName);
        }

        /// <summary>
        /// Try to correct vessel name
        /// </summary>
        /// <param name="masterBillViewModel"></param>
        /// <returns>found(valid)/corrected (valid after corrected data)/notfound(invalid)</returns>
        protected string CorrectVesselName(MasterBillOfLadingViewModel masterBillViewModel)
        {
            if (vessels == null)
            {
                vessels = csfeApiClient.GetActiveVesselsAsync().Result;
            }

            // Compare with exactly case-sensitive 
            var isFound = vessels.Any(v => v.Name.Equals(masterBillViewModel.Vessel, StringComparison.Ordinal));

            // If found, valid data
            if (isFound)
            {
                return "found";
            }
            else
            {
                Func<string, string> dataTransform = (Vessel) =>
                {
                    if (string.IsNullOrEmpty(Vessel))
                    {
                        return null;
                    }
                    return Regex.Replace(Vessel, "[^a-zA-Z0-9]+", "").ToLowerInvariant();
                };

                var matchedVessel = vessels.FirstOrDefault(x => dataTransform(x.Name) == dataTransform(masterBillViewModel.Vessel));
                if (matchedVessel == null)
                {
                    return "notfound";
                }

                // Correct data
                masterBillViewModel.Vessel = matchedVessel.Name;
                masterBillViewModel.VesselFlight = $"{masterBillViewModel.Vessel}/{masterBillViewModel.Voyage ?? string.Empty}";

                return "corrected";

            }
        }

        protected string[] NotAirModeOfTransports = new[]
        {
            ModeOfTransport.Sea,
            ModeOfTransport.Road,
            ModeOfTransport.Railway,
            ModeOfTransport.Courier,

        };

        protected string[] ValidModeOfTransports = new[]
        {
            ModeOfTransport.Sea,
            ModeOfTransport.Air,
            ModeOfTransport.Road,
            ModeOfTransport.Railway,
            ModeOfTransport.Courier
        };
    }
}
