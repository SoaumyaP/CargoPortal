using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class BookingTimelessModel : Entity
    {
        public long Id { get; set; }

        public BuyerComplianceModel BuyerCompliance { get; set; }

        public long BuyerComplianceId { get; set; }
        
        public int? CyEarlyBookingTimeless { get; set; }

        public int? CyLateBookingTimeless { get; set; }

        public int? CfsEarlyBookingTimeless { get; set; }

        public int? CfsLateBookingTimeless { get; set; }

        public int? AirEarlyBookingTimeless { get; set; }

        public int? AirLateBookingTimeless { get; set; }

        public DateForComparison DateForComparison { get; set; }
    }
}