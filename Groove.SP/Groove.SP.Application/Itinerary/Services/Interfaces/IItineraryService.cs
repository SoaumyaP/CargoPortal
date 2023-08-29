using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Itinerary.Services.Interfaces
{
    public interface IItineraryService : IServiceBase<ItineraryModel, ItineraryViewModel>
    {
        Task<IEnumerable<ItineraryViewModel>> GetItinerariesByBOL(long billOfLadingId);

        Task<IEnumerable<ItineraryViewModel>> GetItinerariesByMasterBOL(long masterBillOfLadingId);

        Task<IEnumerable<ItineraryViewModel>> GetItinerariesByContainer(long containerId);

        Task<IEnumerable<ItineraryViewModel>> GetItinerariesByShipmentAsync(long shipmentId);

        Task<IEnumerable<ItineraryViewModel>> GetItinerariesByConsignmentAsync(long consignmentId, string userName, bool isInternal, long userRoleId);

        /// <summary>
        /// Called from application GUI.
        /// To create the new itinerary with an existing consignment (Also create BOL, Master BOL and Container)
        /// </summary>
        Task<ItineraryViewModel> CreateAsync(ItineraryViewModel viewModel, long consignmentId, IdentityInfo currentUser, string affiliates, bool validateHAWB = true);

        /// <summary>
        /// Called from application GUI
        /// </summary>
        Task<ItineraryViewModel> UpdateAsync(ItineraryViewModel viewModel, long consignmentId, IdentityInfo currentUser, string affiliates);

        /// <summary>
        /// Called from application GUI
        /// </summary>
        Task DeleteAsync(long itineraryId, long consignmentId, IdentityInfo currentUser, string affiliates);

        /// <summary>
        /// To change Stage of Booking/PO when update Itinerary via GUI/Api
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <returns></returns>
        Task ChangeStageOfBookingAndPOAsync(long? shipmentId, POFulfillmentModel bookingModel = null);

        /// <summary>
        /// To broadcast CYClosingDate from Itineraries to Freight Scheduler
        /// <br/>
        /// Depend on FS.IsAllowExternalUpdate, the CYClosingDate will broadcast to FS and linked itineraries, bookings.
        /// <br/>
        /// <b>Notes:
        /// <list type="bullet"> This method will run sql script then it should be called after all data saved from EF.</list>
        /// <list type="bullet"> This method called from Itinerary API only.</list>
        /// </b>
        /// </summary>
        /// <param name="itinearyId">Id of Itinerary</param>
        /// <returns></returns>
        Task<bool> BroadcastCYClosingDateAsync(long itinearyId);

        /// <summary>
        /// To update CYClosingDate to POFulfillment, based on 1st Itinerary to broadcast to POFulfillments
        /// </summary>
        /// <param name="consignmentId"></param>
        /// <returns></returns>
        Task<bool> UpdateCYClosingDateToPOFulfillmentAsync(long consignmentId);

    }
}
