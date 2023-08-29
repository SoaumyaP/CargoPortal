using Groove.SP.Application.PurchaseOrders.ViewModels;
using System.Collections.Generic;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class AskMissingPOEmailViewModel
    {
        public AskMissingPOEmailViewModel()
        {
            POs = new List<POEmailDetailViewModel>();
        }

        public string Name { set; get; }
        public long BookingId { set; get; }
        public string BookingNumber { set; get; }
        public string BookingDetailPage { set; get; }
        public string ListOfPOPage { set; get; }
        public string CompanyName { set; get; }
        public string EmailBookingOwner { set; get; }

        public List<POEmailDetailViewModel> POs { set; get; }
    }
}
