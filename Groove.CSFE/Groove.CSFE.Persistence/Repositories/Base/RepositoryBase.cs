// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RepositoryBase.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   Defines the RepositoryBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace Groove.CSFE.Persistence.Repositories.Base
{
    public abstract class RepositoryBase<TContext> : IRepositoryInjection where TContext : DbContext
    {
        protected RepositoryBase(TContext context)
        {
            this.Context = context;
        }
        
        protected TContext Context { get; private set; }

        public IRepositoryInjection SetContext(DbContext context)
        {
            this.Context = (TContext)context;
            return this;
        }
    }
}
