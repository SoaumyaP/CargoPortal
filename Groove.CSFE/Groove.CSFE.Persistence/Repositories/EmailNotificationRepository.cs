using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Persistence.Contexts;
using Groove.CSFE.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.CSFE.Persistence.Repositories
{
    public class EmailNotificationRepository: Repository<CsfeContext, EmailNotificationModel>, IEmailNotificationRepository
    {
        public EmailNotificationRepository(CsfeContext context) : base(context)
        {

        }
    }
}
