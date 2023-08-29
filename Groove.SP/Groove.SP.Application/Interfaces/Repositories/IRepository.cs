// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRepository.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   Repository Interface 
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Groove.SP.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository Interface
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="System.IDisposable" />
    public interface IRepository<TEntity>
    {
        /// <summary>
        /// To get IQueryable of list of entity, only support properties.
        /// Please use method Query/ QueryAsNoTracking to be supported on filtering and ordering
        /// </summary>
        /// <param name="includes">Properties included</param>
        /// <returns>IQueryable of list</returns>
        IQueryable<TEntity> GetListQueryable(Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null);

        /// <summary>
        /// To get IQueryable of list of entity in default tracking mode
        /// </summary>
        /// <param name="filter">filter conditions</param>
        /// <param name="orderBy">order by statements</param>
        /// <param name="includes">properties included</param>
        /// <returns>IQueryable of list</returns>
        IQueryable<TEntity> Query(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null);

        /// <summary>
        /// To get IQueryable of list of entity as No tracking mode.
        /// Use to get reference data
        /// </summary>
        /// <param name="filter">filter conditions</param>
        /// <param name="orderBy">order by statements</param>
        /// <param name="includes">properties included</param>
        /// <returns>IQueryable of list</returns>
        IQueryable<TEntity> QueryAsNoTracking(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null);


        /// <summary>
        /// To get IQueryable of entity in default Tracking mode
        /// </summary>
        /// <param name="filter">Filtered by properties</param>
        /// <param name="orderBy">Ordered by properties</param>
        /// <param name="includes">Included properties, Eager Loading</param>
        /// <returns>Entity</returns>
        Task<TEntity> GetAsync(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null);

        /// <summary>
        /// To get IQueryable of entity in default Tracking mode
        /// </summary>
        /// <param name="filter">Filtered by properties</param>
        /// <param name="orderBy">Ordered by properties</param>
        /// <param name="includes">Included properties, Eager Loading </param>
        /// <returns>Entity</returns>
        Task<TEntity> GetAsNoTrackingAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null);

        /// <summary>
        /// If an entity with the given primary key values is being tracked by the context, then it is returned immediately without making a request to the database. 
        /// Otherwise, a query is made to the database for an entity with the given primary key values.
        /// </summary>
        /// <param name="keys">Array of key</param>
        /// <returns>Entity</returns>
        Task<TEntity> FindAsync(params object[] keys);

        /// <summary>
        /// To add new entity to database context
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>Task</returns>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// To add new entities to database context
        /// </summary>
        /// <param name="entities">Array of entity to add</param>
        /// <returns>Task</returns>
        Task AddRangeAsync(TEntity[] entities);

        /// <summary>
        /// To update entity in database context
        /// </summary>
        /// <param name="entity">Entity to update</param>
        void Update(TEntity entity);

        /// <summary>
        /// To update entities in database context
        /// </summary>
        /// <param name="entities"></param>
        void UpdateRange(TEntity[] entities);

        /// <summary>
        /// To remove entity in database context
        /// </summary>
        /// <param name="entity">Entity to remove</param>
        void Remove(TEntity entity);

        /// <summary>
        /// To remove entities in database context
        /// </summary>
        /// <param name="entities">Array of entity to remove</param>
        /// <returns>Task</returns>
        void RemoveRange(TEntity[] entities);

        /// <summary>
        /// To remove entities in database context by keys
        /// </summary>
        /// <param name="keys">Array of key</param>
        /// <returns>Boolean</returns>
        bool RemoveByKeys(params object[] keys);

        /// <summary>
        /// To check if any entity available
        /// </summary>
        /// <param name="filter">Filtered by properties</param>
        /// <returns>Boolean</returns>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter = null);
    }
}
