using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.Interfaces.Repositories
{
    public interface IPOFulfillmentRepository : IRepository<POFulfillmentModel>
    {
        Task<string> GetNextPOFFSequenceValueAsync();
        Task<string> GetNextPOFFSequenceValueAsync(long customerOrgId);
        Task<string> GetNextPOFFLoadSequenceValueAsync();
        Task ResetPOFFSequenceValueAsync();
        Task ResetPOFFLoadSequenceValueAsync();
    }
}