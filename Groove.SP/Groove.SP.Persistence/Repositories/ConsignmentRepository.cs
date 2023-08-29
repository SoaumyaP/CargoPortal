using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Persistence.Repositories
{
    public class ConsignmentRepository : Repository<SpContext, ConsignmentModel>, IConsignmentRepository
    {
        public ConsignmentRepository(SpContext context)
            : base(context)
        { }
    }
}
