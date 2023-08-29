using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Interfaces.Repositories
{
    public interface ITranslationRepository : IRepository<TranslationModel>
    {
        Task<IEnumerable<TranslationModel>> GetAllAsync();
    }
}
