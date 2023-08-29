using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Common;
using FluentValidation;

namespace Groove.SP.Application.BuyerCompliance.Validations
{
    public class BookingTimelessValidator : BaseValidation<BookingTimelessViewModel>
    {
        public BookingTimelessValidator()
        {
            RuleFor(a => a.CyEarlyBookingTimeless).NotNull().GreaterThan(a => a.CyLateBookingTimeless);
            RuleFor(a => a.CyLateBookingTimeless).NotNull();
            RuleFor(a => a.CfsEarlyBookingTimeless).NotNull().GreaterThan(a => a.CfsLateBookingTimeless);
            RuleFor(a => a.CfsLateBookingTimeless).NotNull();
            RuleFor(a => a.AirEarlyBookingTimeless).NotNull().GreaterThan(a => a.AirLateBookingTimeless);
            RuleFor(a => a.AirLateBookingTimeless).NotNull();
        }
    }
}
