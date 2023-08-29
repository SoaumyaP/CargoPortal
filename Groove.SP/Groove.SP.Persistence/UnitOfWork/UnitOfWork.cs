using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Persistence.Repositories.Base;
using AutoMapper;
using MediatR;

namespace Groove.SP.Persistence.UnitOfWork
{
    public class UnitOfWork: IUnitOfWork
    {
        protected readonly IServiceProvider ServiceProvider;

        protected readonly IMapper Mapper;

        private DbContext context;

        public DbContext Context => context;

        private bool isDisposed;

        private IDbContextTransaction _transaction;

        private readonly IMediator _mediator;

        protected internal UnitOfWork(DbContext context, IServiceProvider serviceProvider, IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.context = context;
            this.ServiceProvider = serviceProvider;
            this.Mapper = serviceProvider.GetService(typeof(IMapper)) as IMapper;

        }

        ~UnitOfWork()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IRepository<TEntity> GetRepository<TEntity>()
        {
            this.CheckDisposed();
            var repositoryType = typeof(IRepository<TEntity>);
            var repository = (IRepository<TEntity>)this.ServiceProvider.GetService(repositoryType);
            if (repository == null)
            {
                // throw new RepositoryNotFoundException(repositoryType.Name, String.Format("Repository {0} not found in the IOC container. Check if it is registered during startup.", repositoryType.Name));
            }

            var repositoryInjection = (IRepositoryInjection)repository;
            repositoryInjection?.SetContext(this.context);
            return repository;
        }

        public TRepository GetCustomRepository<TRepository>()
        {
            this.CheckDisposed();
            var repositoryType = typeof(TRepository);
            var repository = (TRepository)this.ServiceProvider.GetService(repositoryType);
            if (repository == null)
            {
                // throw new RepositoryNotFoundException(repositoryType.Name, String.Format("Repository {0} not found in the IOC container. Check if it is registered during startup.", repositoryType.Name));
            }

            var repositoryInjection = (IRepositoryInjection)repository;
            repositoryInjection?.SetContext(this.context);
            return repository;
        }

        public void SaveChanges()
        {
            try
            {
                this.CheckDisposed();
                this.context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new AppException(ex.Message) { IsDbConcurrencyUpdate = true };
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                this.CheckDisposed();

                // Dispatch Domain Events collection. 
                // Choices:
                // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
                // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
                // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
                // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
                await _mediator.DispatchDomainEventsAsync(this.context);

                await this.context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new AppException(ex.Message) { IsDbConcurrencyUpdate = true };
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.CheckDisposed();

                // Dispatch Domain Events collection. 
                // Choices:
                // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
                // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
                // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
                // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
                await _mediator.DispatchDomainEventsAsync(this.context);

                await this.context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new AppException(ex.Message) { IsDbConcurrencyUpdate = true };
            }
        }

        public void BeginTransaction()
        {
            _transaction = context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _transaction.Commit();
            _transaction.GetDbTransaction().Dispose();
        }

        public void RollbackTransaction()
        {
            _transaction.Rollback();
            _transaction.GetDbTransaction().Dispose();
        }

        public IMapper GetMapper()
        {
            return this.Mapper;
        }

        protected void CheckDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("The UnitOfWork is already disposed and cannot be used anymore.");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    if (this.context != null)
                    {
                        this.context.Dispose();
                        this.context = null;
                    }
                }
            }

            this.isDisposed = true;
        }
    }
}
