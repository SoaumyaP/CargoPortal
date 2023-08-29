using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.FreightScheduler.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Infrastructure.CSFE.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Groove.SP.Application.FreightScheduler.Validations
{
    public class UpdateFreightSchedulerApiViewModelValidator : BaseValidation<UpdateFreightSchedulerApiViewModel>
    {
        protected ICSFEApiClient csfeApiClient;
        protected IEnumerable<Carrier> carriers;
        protected IEnumerable<Vessel> vessels;
        protected IEnumerable<Location> locations;
        private readonly IFreightSchedulerRepository _freightSchedulerRepository;

        public UpdateFreightSchedulerApiViewModelValidator(ICSFEApiClient csfeApiClient, IFreightSchedulerRepository freightSchedulerRepository)
        {
            this.csfeApiClient = csfeApiClient;
            _freightSchedulerRepository = freightSchedulerRepository;

            // ETADate
            RuleFor(a => a.ETADate).NotEmpty();

            // ModeOfTransport
            RuleFor(a => a.ModeOfTransport).NotEmpty();
            RuleFor(a => a.ModeOfTransport)
                .Must(a => ValidModeOfTransports.Contains(a, comparer: StringComparer.InvariantCultureIgnoreCase))
                .WithMessage($"ModeOfTransport is invalid. Supported values are: {string.Join(", ", ValidModeOfTransports)}.")
                .When(a => !string.IsNullOrEmpty(a.ModeOfTransport));

            // CarrierCode
            RuleFor(a => a.CarrierCode).NotEmpty();
            RuleFor(a => a)
                .Must(a => CheckCarrierCodeValid(a))
                .WithMessage("CarrierCode is not existing on master data.")
                .When(a => !string.IsNullOrEmpty(a.CarrierCode) &&
                        !string.IsNullOrEmpty(a.ModeOfTransport));

            // VesselName
            RuleFor(a => a.VesselName)
                .NotEmpty()
                .When(a => !string.IsNullOrEmpty(a.ModeOfTransport)
                    && a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));
            RuleFor(a => a)
                .Must(a => CheckVesselNameValid(a))
                .WithMessage("VesselName is not existing on master data.")
                .When(a => !string.IsNullOrEmpty(a.VesselName)
                    && !string.IsNullOrEmpty(a.ModeOfTransport)
                    && a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

            // Voyage
            RuleFor(a => a.Voyage)
                .NotEmpty()
                .When(a => !string.IsNullOrEmpty(a.ModeOfTransport)
                    && a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

            // LocationToCode
            RuleFor(a => a.LocationToCode).NotEmpty();
            RuleFor(a => a.LocationToCode)
                .Must(a => CheckLocationValid(a))
                .WithMessage("LocationToCode is not existing on master data.")
                .When(a => !string.IsNullOrEmpty(a.LocationToCode));

            // ATA
            RuleFor(a => a.ATADate)
                .LessThanOrEqualTo(x => DateTime.Today)
                .WithMessage("Value must be earlier than or equal to Today")
                .When(c => c.ATADate.HasValue);

            // ATA
            RuleFor(a => a.ATADate)
                .MustAsync(async (viewModel, aTADate, cancellation) => await CheckATAGreaterThanOrEqualToATDAsync(viewModel))
                .WithMessage("Value must be later than or equal to ATDDate")
                .When(c => c.ATADate.HasValue);
        }

        private async Task<bool> CheckATAGreaterThanOrEqualToATDAsync(UpdateFreightSchedulerApiViewModel viewModel)
        {
            var schedules = await _freightSchedulerRepository.QueryAsNoTracking(s =>
            s.ModeOfTransport == viewModel.ModeOfTransport
            && s.CarrierCode == viewModel.CarrierCode
            && s.LocationToCode == viewModel.LocationToCode
            && s.VesselName == viewModel.VesselName
            && s.Voyage == viewModel.Voyage).ToListAsync();

            if (schedules.Count > 0)
            {
                foreach (var item in schedules)
                {
                    if (item.ATDDate.HasValue && viewModel.ATADate < item.ATDDate)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected bool CheckCarrierCodeValid(UpdateFreightSchedulerApiViewModel schedule)
        {
            var data = csfeApiClient.GetAllCarriesAsync().Result;
            if (carriers == null)
            {
                carriers = csfeApiClient.GetAllCarriesAsync().Result;
            }
            var result = true;

            result = carriers.Any(c => c.CarrierCode == schedule.CarrierCode && !string.IsNullOrEmpty(c.ModeOfTransport) && c.ModeOfTransport.Equals(schedule.ModeOfTransport, StringComparison.InvariantCultureIgnoreCase));

            return result;
        }

        protected bool CheckVesselNameValid(UpdateFreightSchedulerApiViewModel schedule)
        {
            var tryToCorrectVesselName = CorrectVesselName(schedule);
            switch (tryToCorrectVesselName)
            {
                case "found":
                    return true;
                case "corrected":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Try to correct vessel name
        /// </summary>
        /// <param name="itinerary"></param>
        /// <returns>found(valid)/corrected (valid after corrected data)/notfound(invalid)</returns>
        protected string CorrectVesselName(UpdateFreightSchedulerApiViewModel schedule)
        {
            if (vessels == null)
            {
                vessels = csfeApiClient.GetActiveVesselsAsync().Result;
            }

            // filter to get real vessels
            vessels = vessels.Where(x => x.IsRealVessel).ToList();

            // Compare with exactly case-sensitive 
            var isFound = vessels.Any(v => v.Name.Equals(schedule.VesselName, StringComparison.Ordinal));

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

                var matchedVessel = vessels.FirstOrDefault(x => dataTransform(x.Name) == dataTransform(schedule.VesselName));
                if (matchedVessel == null)
                {
                    return "notfound";
                }

                // Correct data
                schedule.VesselName = matchedVessel.Name;

                return "corrected";

            }
        }

        protected bool CheckLocationValid(string location)
        {
            if (locations == null)
            {
                locations = csfeApiClient.GetAllLocationsAsync().Result;
            }

            return locations.Any(l => l.Name == location);
        }

        protected string[] ValidModeOfTransports = new[]
        {
            ModeOfTransport.Sea
        };
    }
}