// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceBase.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   Service Base Class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Groove.CSFE.Application.Exceptions;
using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Groove.CSFE.Application.Common
{
    /// <summary>
    /// Service Base Class
    /// </summary>
    /// <typeparam name="TModel">The model.</typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    public class ServiceBase<TModel, TViewModel> : IServiceBase<TModel, TViewModel> where TModel : Entity where TViewModel : ViewModelBase<TModel>
    {
        #region Protected Properties

        protected readonly IUnitOfWorkProvider UnitOfWorkProvider;

        /// <summary>
        /// The unit of work.
        /// </summary>
        protected readonly IUnitOfWork UnitOfWork;

        /// <summary>
        /// Repository of <see cref="TModel"/>.
        /// </summary>
        protected readonly IRepository<TModel> Repository;

        protected readonly IMapper Mapper;

        /// <summary>
        /// Custom sort fields mapping for list
        /// </summary>
        protected virtual IDictionary<string, string> SortMap { get; } = new Dictionary<string, string>() { { "statusName", "status" } };
                
        protected virtual Func<IQueryable<TModel>, IQueryable<TModel>> IncludeProperties { get; }

        #endregion

        #region Constructors

        public ServiceBase(IUnitOfWorkProvider unitOfWorkProvider)
        {
            UnitOfWorkProvider = unitOfWorkProvider;
            this.UnitOfWork = unitOfWorkProvider.CreateUnitOfWork();
            this.Repository = this.UnitOfWork.GetRepository<TModel>();
            this.Mapper = this.UnitOfWork.GetMapper();
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public virtual async Task<TViewModel> GetByKeysAsync(params object[] keys)
        {
            var model = await this.Repository.FindAsync(keys);
            return Mapper.Map<TViewModel>(model);
        }

        public virtual async Task<DataSourceResult> ListAsync(DataSourceRequest request)
        {
            var result = await GetListAsync(request, IncludeProperties);
            return result;
        }

        public virtual async Task<DataSourceResult> GetListAsync(DataSourceRequest request, Func<IQueryable<TModel>, IQueryable<TModel>> includes = null)
        {   
            if (request.Sorts != null)
            {
                foreach (var sortDes in request.Sorts)
                {
                    if (SortMap.ContainsKey(sortDes.Member))
                    {
                        sortDes.Member = SortMap[sortDes.Member];
                    }
                }
            }

            var query = this.Repository.GetListQueryable(includes).ProjectTo<TViewModel>(Mapper.ConfigurationProvider);
            var result = await query.ToDataSourceResultAsync(request);
            return result;
        }

        public virtual async Task<IEnumerable<TViewModel>> GetAllAsync()
        {
            var models = await this.Repository.Query().ToListAsync();
            return Mapper.Map<IEnumerable<TViewModel>>(models);
        }

        public virtual async Task<TViewModel> CreateAsync(TViewModel viewModel)
        {
            viewModel.ValidateAndThrow();

            TModel model = Mapper.Map<TModel>(viewModel);

            var error = await ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            await this.Repository.AddAsync(model);
            await this.UnitOfWork.SaveChangesAsync();

            OnEntityCreated(model);

            viewModel = Mapper.Map<TViewModel>(model);
            return viewModel;
        }

        public virtual async Task<TViewModel> UpdateAsync(TViewModel viewModel)
        {
            viewModel.ValidateAndThrow();

            TModel model = Mapper.Map<TModel>(viewModel); 

            var error = await this.ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            this.OnEntityUpdated(model);

            viewModel = Mapper.Map<TViewModel>(model);
            return viewModel;
        }

        public virtual async Task<TViewModel> UpdateAsync(TViewModel viewModel, params object[] keys)
        {
            viewModel.ValidateAndThrow(true);

            TModel model = await this.Repository.FindAsync(keys);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", keys)} not found!");
            }

            Mapper.Map(viewModel, model);

            var error = await this.ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            this.OnEntityUpdated(model);

            viewModel = Mapper.Map<TViewModel>(model);
            return viewModel;
        }

        public virtual async Task<bool> DeleteAsync(TViewModel viewModel)
        {
            TModel model = Mapper.Map<TModel>(viewModel); 
            this.Repository.Remove(model);
            await this.UnitOfWork.SaveChangesAsync();

            OnEntityDeleted(model);

            return true;
        }

        public virtual async Task<bool> DeleteAsync(params object[] keys)
        {
            if(!this.Repository.Remove(keys))
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", keys)} not found!");
            }

            await this.UnitOfWork.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Protected Methods

        protected virtual Task<string> ValidateDatabaseBeforeAddOrUpdateAsync(TModel model)
        {
            return Task.FromResult(string.Empty);
        }

        protected virtual void OnEntityCreated(TModel model)
        {
        }

        protected virtual void OnEntityUpdated(TModel model)
        {
        }

        protected virtual void OnEntityDeleted(TModel model)
        {
        }

        #endregion
    }
}
