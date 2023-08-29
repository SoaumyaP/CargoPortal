using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.Core.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Organizations.Services.Interfaces
{
    public interface IOrganizationListService
    {
        Task<DataSourceResult> GetListSupplierRelationshipAsync(DataSourceRequest request, long customerRelationshipId);

        Task<IList<CustomerRelationshipQueryModel>> GetListCustomerRelationshipAsync(long supplierRelationshipId);

        Task<DataSourceResult> GetListOrganizationAsync(DataSourceRequest request);

        Task<IList<CustomerRelationshipQueryModel>> GetSuppliersAsync(long customerId);
    }
}
