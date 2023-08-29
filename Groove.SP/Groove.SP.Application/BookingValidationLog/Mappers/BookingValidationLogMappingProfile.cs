using Groove.SP.Application.BookingValidationLog.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BookingValidationLog.Mappers
{
    public class BookingValidationLogMappingProfile : MappingProfileBase<BookingValidationLogModel, BookingValidationLogViewModel>
    {
        public BookingValidationLogMappingProfile()
        {
            CreateMap<BookingValidationLogModel, BookingValidationLogViewModel>().ReverseMap();
        }
    }
}
