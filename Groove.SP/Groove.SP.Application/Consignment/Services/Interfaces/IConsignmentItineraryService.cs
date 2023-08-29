using Groove.SP.Application.Common;
using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.Consignment.Services.Interfaces
{
    public interface IConsignmentItineraryService : IServiceBase<ConsignmentItineraryModel, ConsignmentItineraryViewModel>
    {

        /// <summary>
        /// Called via ConsignmentItinerary API (by ediSON) to confirm Itinerary to Shipment.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ConsignmentItineraryViewModel> ConfirmItineraryToShipmentAsync(ConsignmentItineraryViewModel model);
    }
}
