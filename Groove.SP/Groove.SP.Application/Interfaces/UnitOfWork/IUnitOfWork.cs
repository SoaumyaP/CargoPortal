// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUnitOfWork.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   Unit of Work Interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Groove.SP.Application.Interfaces.Repositories;

namespace Groove.SP.Application.Interfaces.UnitOfWork
{
    /// <summary>
    /// Unit of Work Interface
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IUnitOfWork : IDisposable
    {
        void SaveChanges();

        Task SaveChangesAsync();

        Task SaveChangesAsync(CancellationToken cancellationToken);

        IRepository<TEntity> GetRepository<TEntity>();

        TRepository GetCustomRepository<TRepository>();

        void BeginTransaction();

        void CommitTransaction();

        void RollbackTransaction();

        IMapper GetMapper();
    }
}
