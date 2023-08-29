using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Persistence.Repositories
{
    public class TranslationRepository : Repository<SpContext, TranslationModel>, ITranslationRepository
    {
        private readonly SpContext _spContext;

        public TranslationRepository(SpContext context)
            : base(context)
        {
            _spContext = context;
        }

        public async Task<IEnumerable<TranslationModel>> GetAllAsync()
        {
            var translations = await this._spContext.Translations.ToListAsync();
            return translations;
        }
    }
}
