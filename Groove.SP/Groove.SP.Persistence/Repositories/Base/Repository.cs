// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Repository.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//    Defines the Repository type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core;

namespace Groove.SP.Persistence.Repositories.Base
{
    public abstract class Repository<TContext, TEntity> : RepositoryBase<TContext>, IRepository<TEntity>
        where TContext : DbContext where TEntity : Entity, new()
    {
        #region Constructors

        public Repository(TContext context) : base(context)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Only override this for Database View
        /// </summary>
        protected virtual string ViewSql { get; } = null;

        #endregion

        #region Protected Methods
        protected virtual IQueryable<TEntity> GetQueryable()
        {
            DbSet<TEntity> dbSet = Context.Set<TEntity>();
            IQueryable<TEntity> query;

            if (!string.IsNullOrEmpty(ViewSql))
            {
                query = dbSet.FromSqlRaw(ViewSql);
            }
            else
            {
                query = dbSet;
            }

            return query;
        }

        protected virtual IQueryable<TEntity> QueryDb(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, Func<IQueryable<TEntity>, IQueryable<TEntity>> includes)
        {
            IQueryable<TEntity> query = GetQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null)
            {
                query = includes(query);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return query;
        }
        #endregion

        #region Public Methods

        public virtual IQueryable<TEntity> GetListQueryable(Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null)
        {
            var query = this.GetQueryable();

            if (includes != null)
            {
                query = includes(query);
            }

            return query;
        }

        public virtual IQueryable<TEntity> Query(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null)
        {
            var result = QueryDb(filter, orderBy, includes);
            return result;
        }

        public virtual IQueryable<TEntity> QueryAsNoTracking(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null)
        {
            var result = QueryDb(filter, orderBy, includes).AsNoTracking();
            return result;
        }

        public virtual async Task<TEntity> FindAsync(params object[] keys)
        {
            return await Context.Set<TEntity>().FindAsync(keys);
        }

        public virtual async Task<TEntity> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null)
        {
            var result = QueryDb(filter, orderBy, includes);
            return await result.FirstOrDefaultAsync();
        }


        public virtual async Task<TEntity> GetAsNoTrackingAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null)
        {
            var result = QueryDb(filter, orderBy, includes).AsNoTracking();
            return await result.FirstOrDefaultAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new InvalidOperationException("Unable to add a null entity to the repository.");
            }

            await Context.Set<TEntity>().AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(TEntity[] entities)
        {
            if (entities == null)
            {
                throw new InvalidOperationException("Unable to add a null entities to the repository.");
            }

            Context.ChangeTracker.AutoDetectChangesEnabled = false;
            await Context.Set<TEntity>().AddRangeAsync(entities);
            Context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        public virtual void Update(TEntity entity)
        {
            Context.Set<TEntity>().Update(entity);
        }

        public virtual void UpdateRange(TEntity[] entities)
        {
            Context.Set<TEntity>().UpdateRange(entities);
        }

        public virtual void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Entry(entity).State = EntityState.Deleted;
            Context.Set<TEntity>().Remove(entity);
        }

        public virtual void RemoveRange(TEntity[] entities)
        {
            if (entities == null)
            {
                throw new InvalidOperationException("Unable to remove a null entities.");
            }
            Context.Set<TEntity>().RemoveRange(entities);
        }

        public virtual bool RemoveByKeys(params object[] keys)
        {
            var databaseSet = this.Context.Set<TEntity>();
            var entity = databaseSet.Find(keys);

            if (entity != null)
            {
                databaseSet.Remove(entity);
                return true;
            }
            return false;
        }

        public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            DbSet<TEntity> dbSet = Context.Set<TEntity>();
            IQueryable<TEntity> query = null;

            if (!string.IsNullOrEmpty(ViewSql))
            {
                query = dbSet.FromSqlRaw(ViewSql);
            }

            if (filter != null)
            {
                if (query != null)
                {
                    query = query.Where(filter).AsNoTracking();
                }
                else
                {
                    query = dbSet.Where(filter).AsNoTracking();
                }
            }

            return query.AnyAsync();
        }
        #endregion

    }
}
