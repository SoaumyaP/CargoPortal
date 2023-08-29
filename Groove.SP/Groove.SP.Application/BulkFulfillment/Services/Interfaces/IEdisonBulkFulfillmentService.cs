using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.BulkFulfillment.Services.Interfaces
{
    public interface IEdisonBulkFulfillmentService
    {
        Task<POFulfillmentBookingRequestModel> CreateBookingRequest(string userName, POFulfillmentModel bulkBooking);
        Task LoginToEdisonAsync();
        Task SendEBookingToEdisonAsync();

        /// <summary>
        /// Send eSI request to ediSON.
        /// </summary>
        /// <param name="poff"></param>
        /// <param name="bookingRequest"></param>
        /// <returns></returns>
        Task ProcesseSIAsync(POFulfillmentModel poff, POFulfillmentBookingRequestModel bookingRequest);
    }
}
