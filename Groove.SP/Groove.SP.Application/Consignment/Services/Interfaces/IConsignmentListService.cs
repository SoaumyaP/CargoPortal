using Groove.SP.Core.Data;
using System.Threading.Tasks;

namespace Groove.SP.Application.Consignment.Services.Interfaces
{
    public interface IConsignmentListService
    {
        Task<DataSourceResult> GetListConsignmentAsync(DataSourceRequest request, bool isInternal, string affiliates = "");
    }
}
