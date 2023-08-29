using System.Threading.Tasks;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.POFulfillment.Services.Interfaces
{
    public interface IEdiSonBookingService
    {
        Task<POFulfillmentBookingRequestModel> CreateBookingRequestAsync(string userName, POFulfillmentModel poff, bool sendToEdison = true);
        Task CancelBookingRequest(POFulfillmentBookingRequestModel bookingRequest);
    }
}