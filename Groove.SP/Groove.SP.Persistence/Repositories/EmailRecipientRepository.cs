using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities.Email;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;

namespace Groove.SP.Persistence.Repositories
{
    public class EmailRecipientRepository : Repository<SpContext, EmailRecipientModel>, IEmailRecipientRepository
    {
        public EmailRecipientRepository(SpContext context)
            : base(context)
        {}
    }
}
