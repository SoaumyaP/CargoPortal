using Groove.SP.Application.BookingValidationLog.Services.Interfaces;
using Groove.SP.Application.BookingValidationLog.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.BookingValidationLog.Services
{
    public class BookingValidationLogService : ServiceBase<BookingValidationLogModel, BookingValidationLogViewModel>, IBookingValidationLogService
    {
        public BookingValidationLogService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        {
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, long parentId)
        {
            var result = await GetListAsync(request, null, x => x.BuyerComplianceId == parentId);
            return result;
        }
    }
}
