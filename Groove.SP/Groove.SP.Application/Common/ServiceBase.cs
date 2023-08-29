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
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core;
using Groove.SP.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Groove.SP.Application.Common
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

        protected virtual Func<IQueryable<TModel>, IQueryable<TModel>> FullIncludeProperties { get; }

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

        public virtual async Task<DataSourceResult> ListAsync(DataSourceRequest request)
        {
            var result = await GetListAsync(request, FullIncludeProperties);
            return result;
        }
        public void ModifyFilters(IList<IFilterDescriptor> filters)
        {
            if (filters != null)
            {
                for (int i = 0; i < filters.Count; i++)
                {

                    if (filters[i] is CompositeFilterDescriptor)
                    {
                        ModifyFilters(((CompositeFilterDescriptor)filters[i]).FilterDescriptors);
                    }
                    else
                    {
                        var descriptor = filters[i] as FilterDescriptor;
                        if (descriptor.Member.EndsWith("Date"))
                        {
                            if (descriptor.Operator == FilterOperator.IsEqualTo)
                            {
                                var date = ((DateTime)descriptor.Value);
                                descriptor.Operator = FilterOperator.IsLessThanOrEqualTo;
                                descriptor.Value = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
                                filters.Add(new FilterDescriptor
                                {
                                    Member = descriptor.Member,
                                    Operator = FilterOperator.IsGreaterThanOrEqualTo,
                                    Value = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0)
                                }
                                );
                            }
                            else if (descriptor.Operator == FilterOperator.IsNotEqualTo)
                            {
                                var date = ((DateTime)descriptor.Value);
                                var member = descriptor.Member;
                                // remove existing filterOperator
                                filters.Remove(descriptor);

                                // Add new composite filter with logical 'OR'
                                filters.Add(new CompositeFilterDescriptor()
                                {
                                    LogicalOperator = FilterCompositionLogicalOperator.Or,
                                    FilterDescriptors = new FilterDescriptorCollection()
                                    {
                                        new FilterDescriptor() {
                                            Member = member,
                                            Operator = FilterOperator.IsGreaterThan,
                                            Value = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999)
                                        },
                                        new FilterDescriptor() {
                                            Member = member,
                                            Operator = FilterOperator.IsLessThan,
                                            Value = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0)
                                        },
                                    }
                                });

                            }
                            else if (descriptor.Operator == FilterOperator.IsLessThanOrEqualTo)
                            {
                                var date = ((DateTime)descriptor.Value);
                                descriptor.Value = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
                            }
                            else if (descriptor.Operator == FilterOperator.IsGreaterThan)
                            {
                                var date = ((DateTime)descriptor.Value);
                                descriptor.Value = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
                            }
                            else if (descriptor.Operator == FilterOperator.IsGreaterThanOrEqualTo)
                            {
                                var date = ((DateTime)descriptor.Value);
                                descriptor.Value = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
                            }
                        }
                    }
                }

            }

        }

        public void ModifySorts(IList<SortDescriptor> sorts)
        {
            if (sorts != null)
            {
                foreach (var sortDes in sorts)
                {
                    if (SortMap.ContainsKey(sortDes.Member))
                    {
                        sortDes.Member = SortMap[sortDes.Member];
                    }
                }
            }
        }

        public virtual async Task<DataSourceResult> GetListAsync(DataSourceRequest request, Func<IQueryable<TModel>, IQueryable<TModel>> includes = null, Expression<Func<TModel, bool>> customFilters = null)
        {
            ModifySorts(request.Sorts);

            // Custom filter for field '..Date' only
            ModifyFilters(request.Filters);

            var query = this.Repository.GetListQueryable(includes);
            if (customFilters != null)
            {
                query = query.Where(customFilters);
            }

            var result = await query
                .ProjectTo<TViewModel>(Mapper.ConfigurationProvider)
                .ToDataSourceResultAsync(request);
            return result;
        }

        public virtual async Task<IEnumerable<TViewModel>> GetAllAsync()
        {
            var models = await this.Repository.Query().ToListAsync();
            return Mapper.Map<IEnumerable<TViewModel>>(models);
        }

        public virtual async Task<TViewModel> GetAsync(params object[] keys)
        {
            var model = await this.Repository.FindAsync(keys);
            return Mapper.Map<TViewModel>(model);
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

        public virtual async Task<TViewModel> UpdateAsync(TViewModel viewModel, params object[] keys)
        {
            viewModel.ValidateAndThrow(true);

            TModel model = await this.Repository.FindAsync(keys);

            if(model == null)
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

        public virtual async Task<bool> DeleteByKeysAsync(params object[] keys)
        {
            var isDeleted = this.Repository.RemoveByKeys(keys);
            if (!isDeleted)
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
