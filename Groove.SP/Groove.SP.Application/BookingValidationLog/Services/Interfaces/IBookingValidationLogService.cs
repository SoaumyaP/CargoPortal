using Groove.SP.Application.BookingValidationLog.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.BookingValidationLog.Services.Interfaces
{
    public interface IBookingValidationLogService : IServiceBase<BookingValidationLogModel, BookingValidationLogViewModel>
    {
        Task<DataSourceResult> ListAsync(DataSourceRequest request, long parentId);
    }
}
