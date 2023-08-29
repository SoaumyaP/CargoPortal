using Groove.CSFE.Core.Entities;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Interfaces.Repositories
{
    public interface IOrganizationRepository : IRepository<OrganizationModel>
    {
        Task<string> GetNextNumberAsync();

        /// <summary>
        /// To reseve a numbers of sequence values
        /// </summary>
        /// <param name="quantityToReserve">A number that is needed to reserved</param>
        /// <returns>A next sequence value</returns>
        Task<int> ReserveSequenceNumberAsync(int quantityToReserve);
    }
}
