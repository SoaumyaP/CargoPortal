using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class BookingTimelessViewModel : ViewModelBase<BookingTimelessModel>
    {
        public long Id { get; set; }

        public int? CyEarlyBookingTimeless { get; set; }

        public int? CyLateBookingTimeless { get; set; }

        public int? CfsEarlyBookingTimeless { get; set; }

        public int? CfsLateBookingTimeless { get; set; }

        public int? AirEarlyBookingTimeless { get; set; }

        public int? AirLateBookingTimeless { get; set; }

        public DateForComparison DateForComparison { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
