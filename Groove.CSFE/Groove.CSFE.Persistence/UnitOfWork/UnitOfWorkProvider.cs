// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitOfWorkProvider.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   Unit Of Work Provider Class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Persistence.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Groove.CSFE.Persistence.UnitOfWork
{
    public class UnitOfWorkProvider : IUnitOfWorkProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public UnitOfWorkProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IUnitOfWork CreateUnitOfWork(bool trackChanges = true, bool enableLogging = false)
        {
            var context = (DbContext)_serviceProvider.GetService(typeof(CsfeContext));

            if (!trackChanges)
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }

            var uow = new UnitOfWork(context, _serviceProvider);
            return uow;
        }

        public IUnitOfWork CreateUnitOfWorkForBackgroundJob(bool trackChanges = true, bool enableLogging = false)
        {
            var context = (DbContext)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(CsfeContext));

            if (!trackChanges)
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }

            var uow = new UnitOfWork(context, _serviceProvider);
            return uow;
        }
    }
}
