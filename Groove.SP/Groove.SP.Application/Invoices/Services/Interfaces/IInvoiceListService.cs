
using Groove.SP.Application.Common;
using Groove.SP.Core.Data;
using System.Threading.Tasks;

namespace Groove.SP.Application.Invoices.Services.Interfaces
{
    public interface IInvoiceListService
    {
        Task<DataSourceResult> GetListInvoiceAsync(DataSourceRequest request, IdentityInfo currentUser);
    }
}
