
using Groove.CSFE.Application.Organizations.ViewModels;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Organizations.Services.Interfaces
{
    public interface ICustomerRelationshipService
    {
        Task UpdateAsync(long customerId, long supplierId, CustomerRelationshipViewModel viewModel, string userName);
    }
}
