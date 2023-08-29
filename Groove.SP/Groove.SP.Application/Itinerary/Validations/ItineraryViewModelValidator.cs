using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Infrastructure.CSFE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Groove.SP.Application.Itinerary.Validations
{
    public class ItineraryViewModelValidator<T> : BaseValidation<T> where T : ItineraryViewModel
    {
        protected IFreightSchedulerRepository _freightSchedulerRepository;
        protected ICSFEApiClient csfeApiClient;
        protected IEnumerable<Carrier> carriers;
        protected IEnumerable<Location> locations;
        protected IEnumerable<Vessel> vessels;

        private FreightSchedulerModel LinkedScheduler { set; get; }
        private bool IsLoadedScheduler { set; get; }

        protected void RegisterBussinessValidations()
        {
            // Must of transport not be "Multi-model"
            RuleFor(a => a.ModeOfTransport)
                .Must(a => ValidModeOfTransports.Contains(a, comparer: StringComparer.InvariantCultureIgnoreCase))
                .WithMessage($"Mode of transport is invalid. Supported values are: {string.Join(", ", ValidModeOfTransports)}.")
                .When(a => !string.IsNullOrEmpty(a.ModeOfTransport));

            // Check SCAC if "Sea"
            RuleFor(a => a)
                .Must(a => CheckCarrierCodeValid(a))
                .WithMessage("SCAC is not existing on master data, must be carrier code.")
                .When(
                    a => !string.IsNullOrEmpty(a.SCAC) &&
                    !string.IsNullOrEmpty(a.ModeOfTransport) &&
                    a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase)
                    );

            // Check AirlineCode if "Air"
            RuleFor(a => a)
                .Must(a => CheckCarrierCodeValid(a))
                .WithMessage("Airline code is not existing on master data, must be carrier code.")
                .When(
                    a => !string.IsNullOrEmpty(a.AirlineCode) &&
                    !string.IsNullOrEmpty(a.ModeOfTransport) &&
                    a.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase));

            // Check Loading port
            RuleFor(a => a)
                .Must(a => CheckLoadingPortValid(a.LoadingPort, a.IsCalledFromApp))
                .WithMessage("Loading port is not existing on master data, must be location code.")
                .When(
                    a => !string.IsNullOrEmpty(a.LoadingPort)
                    && (
                        a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) ||
                        a.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase)
                    )
                    );

            // Check Discharge port
            RuleFor(a => a)
                .Must(a => CheckDischargePortValid(a.DischargePort, a.IsCalledFromApp))
                .WithMessage("Discharge port is not existing on master data, must be location code.")
                .When(
                    a => !string.IsNullOrEmpty(a.DischargePort)
                    && (
                        a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) ||
                        a.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase)
                    )
                    );

            // Check ETA Date
            RuleFor(a => a)
                .Must(a => a.ETADate.CompareTo(a.ETDDate) >= 0)
                .WithMessage("ETA must be later than or equal to ETD.");
        }

        protected bool CheckCarrierCodeValid(ItineraryViewModel itinerary)
        {
            if (carriers == null)
            {
                carriers = csfeApiClient.GetAllCarriesAsync().Result;
            }
            var result = true;


            // Not "Air", compare on SCSC and mode of transport
            if (itinerary.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase))
            {
                result = carriers.Any(c => c.CarrierCode == itinerary.SCAC && !string.IsNullOrEmpty(c.ModeOfTransport) && c.ModeOfTransport.Equals(itinerary.ModeOfTransport, StringComparison.InvariantCultureIgnoreCase));
            }
            // Air, compare on AirlineCode and mode of transport
            else if (itinerary.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
            {
                result = carriers.Any(c => c.CarrierCode == itinerary.AirlineCode && !string.IsNullOrEmpty(c.ModeOfTransport) && c.ModeOfTransport.Equals(itinerary.ModeOfTransport, StringComparison.InvariantCultureIgnoreCase));
            }

            return result;
        }

        protected bool CheckCarrierCodeValid(string carrierCode, string modeOfTransport)
        {
            if (carrierCode == null)
            {
                return true;
            }
            if (modeOfTransport == null)
            {
                return true;
            }

            if (carriers == null)
            {
                carriers = csfeApiClient.GetAllCarriesAsync().Result;
            }

            return carriers.Any(x => x.CarrierCode == carrierCode && x.ModeOfTransport.Equals(modeOfTransport, StringComparison.InvariantCultureIgnoreCase) && x.Status == CarrierStatus.Active);
        }

        protected bool CheckVesselNameValid(string vesselName)
        {
            if (string.IsNullOrEmpty(vesselName))
            {
                return true;
            }

            if (vessels == null)
            {
                vessels = csfeApiClient.GetActiveVesselsAsync().Result;
            }

            return vessels.Any(x => x.Name.Equals(vesselName, StringComparison.InvariantCultureIgnoreCase) && x.IsRealVessel);
        }

        protected bool CheckLoadingPortValid(string loadingPort, bool isCalledFromApp)
        {
            if (locations == null)
            {
                locations = csfeApiClient.GetAllLocationsAsync().Result;
            }
            // Call from application cs portal, loadingPort is location description
            if (isCalledFromApp)
            {
                return locations.Any(l => l.LocationDescription == loadingPort);
            }
            return locations.Any(l => l.Name == loadingPort);

        }

        protected bool CheckDischargePortValid(string dischargePort, bool isCalledFromApp)
        {
            if (locations == null)
            {
                locations = csfeApiClient.GetAllLocationsAsync().Result;
            }
            // Call from application cs portal, dischargePort is location description
            if (isCalledFromApp)
            {
                return locations.Any(l => l.LocationDescription == dischargePort);
            }
            return locations.Any(l => l.Name == dischargePort);
        }

        /// <summary>
        /// Try to correct vessel name
        /// </summary>
        /// <param name="itinerary"></param>
        /// <returns>found(valid)/corrected (valid after corrected data)/notfound(invalid)</returns>
        protected string CorrectVesselName(ItineraryViewModel itinerary)
        {
            if (vessels == null)
            {
                vessels = csfeApiClient.GetActiveVesselsAsync().Result;
            }

            // Compare with exactly case-sensitive 
            var isFound = vessels.Any(v => v.Name.Equals(itinerary.VesselName, StringComparison.Ordinal));

            // If found, valid data
            if (isFound)
            {
                return "found";
            }
            else
            {
                Func<string, string> dataTransform = (vesselName) =>
                {
                    if (string.IsNullOrEmpty(vesselName))
                    {
                        return null;
                    }
                    return Regex.Replace(vesselName, "[^a-zA-Z0-9]+", "").ToLowerInvariant();
                };

                var matchedVessel = vessels.FirstOrDefault(x => dataTransform(x.Name) == dataTransform(itinerary.VesselName));
                if (matchedVessel == null)
                {
                    return "notfound";
                }

                // Correct data
                itinerary.VesselName = matchedVessel.Name;
                itinerary.VesselFlight = $"{itinerary.VesselName}/{itinerary.Voyage ?? string.Empty}";

                return "corrected";

            }
        }

        protected void ValidateATDATA()
        {
            //1 ATD has value
            RuleFor(c => c.ATDDate)
                .LessThanOrEqualTo(c => DateTime.Today)
                .WithMessage("Value must be earlier than or equal to Today")
                .When(c => c.ATDDate.HasValue)
                .When(c => c.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

            //2 ATA has value
            RuleFor(c => c.ATADate)
                .LessThanOrEqualTo(c => DateTime.Today)
                .WithMessage("Value must be earlier than or equal to Today")
                .When(c => c.ATADate.HasValue)
                .When(c => c.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

            //3 ATA & ATD have value
            RuleFor(c => c.ATADate)
                .GreaterThanOrEqualTo(c => c.ATDDate)
                .WithMessage("Value must be later than or equal to ATDDate")
                .When(c => c.ATDDate.HasValue && c.ATADate.HasValue)
                .When(c => c.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

            //4 Add/Update ATD (ATD has value)
            RuleFor(c => c.ATDDate)
               .LessThanOrEqualTo(c => LinkedScheduler.ATADate)
               .WithMessage("Value must be earlier than or equal to ATADate")
               .WhenAsync(async (c, cancel) =>
                c.ATDDate.HasValue
                && c.IsPropertyDirty(nameof(ItineraryViewModel.ATADate)) == false
                && await ItineraryLinkedToScheduler(c, null, true))
               .When(c => c.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

            //7.3 Add/Update ATA
            RuleFor(c => c.ATADate)
             .GreaterThanOrEqualTo(c => LinkedScheduler.ATDDate)
             .WithMessage("Value must be later than or equal to ATDDate")
             .WhenAsync(async (c, cancel) =>
              c.IsPropertyDirty(nameof(ItineraryViewModel.ATDDate)) == false
              && c.ATADate.HasValue
              && await ItineraryLinkedToScheduler(c, true, null))
             .When(c => c.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));
        }

        private async Task<bool> ItineraryLinkedToScheduler(ItineraryViewModel viewModel, bool? isATDHasValue = null, bool? isATAHasValue = null)
        {
            if (IsLoadedScheduler == false)
            {
                string loadingPort = null;
                string dischargePort = null;

                var checkingModeOfTransports = new[] { ModeOfTransport.Sea, ModeOfTransport.Air };
                // Itinerary Import/Update API will pass location code
                // Then, need to get location description by location code if Sea or Air
                if (checkingModeOfTransports.Contains(viewModel.ModeOfTransport.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase)
                    & (!string.IsNullOrEmpty(viewModel.LoadingPort) || !string.IsNullOrEmpty(viewModel.DischargePort))
                )
                {
                    if (locations == null)
                    {
                        this.locations = await this.csfeApiClient.GetAllLocationsAsync();
                    }

                    loadingPort = string.IsNullOrEmpty(viewModel.LoadingPort)
                        ? viewModel.LoadingPort
                        : this.locations.First(x => x.Name == viewModel.LoadingPort).LocationDescription;
                    dischargePort = string.IsNullOrEmpty(viewModel.DischargePort)
                        ? viewModel.DischargePort
                        : this.locations.First(x => x.Name == viewModel.DischargePort).LocationDescription;
                }

                LinkedScheduler = await _freightSchedulerRepository.GetAsync(
                fs => fs.ModeOfTransport == ModeOfTransport.Sea
                      && fs.CarrierCode == viewModel.SCAC
                      && fs.VesselName == viewModel.VesselName
                      && fs.Voyage == viewModel.Voyage
                      && fs.LocationFromName == loadingPort
                      && fs.LocationToName == dischargePort);
                IsLoadedScheduler = true;
            }

            var islinkedScheduler = LinkedScheduler == null ? false : true;
            if (islinkedScheduler == false)
            {
                return false;
            }
            else
            {
                if (isATDHasValue == true)
                {
                    return islinkedScheduler && LinkedScheduler.ATDDate.HasValue;
                }

                if (isATAHasValue == true)
                {
                    return islinkedScheduler && LinkedScheduler.ATADate.HasValue;

                }

                return true;
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

        protected string[] ValidHouseBillType = new[]
        {
            BillOfLadingType.HBL,
            BillOfLadingType.FCR,
            BillOfLadingType.SeawayBill,
            BillOfLadingType.TelexRelease
        };
    }
}
