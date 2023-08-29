using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Persistence.Repositories
{
    public class ActivityRepository : Repository<SpContext, ActivityModel>, IActivityRepository
    {
        public ActivityRepository(SpContext context)
            : base(context)
        { }
    }
}
