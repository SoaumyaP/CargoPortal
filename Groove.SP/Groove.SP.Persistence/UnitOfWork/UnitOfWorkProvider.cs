// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitOfWorkProvider.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   Unit Of Work Provider Class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Groove.SP.Persistence.UnitOfWork
{
    public class UnitOfWorkProvider : IUnitOfWorkProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;

        public UnitOfWorkProvider(IServiceProvider serviceProvider, IMediator mediator)
        {
            _serviceProvider = serviceProvider;
            _mediator = mediator;
        }

        public IUnitOfWork CreateUnitOfWork(bool trackChanges = true, bool enableLogging = false)
        {
            var context = (DbContext)_serviceProvider.GetService(typeof(SpContext));

            if (!trackChanges)
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }

            var uow = new UnitOfWork(context, _serviceProvider, _mediator);
            return uow;
        }

        public IUnitOfWork CreateUnitOfWorkForBackgroundJob(bool trackChanges = true, bool enableLogging = false)
        {
            var context = (DbContext)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(SpContext));

            if (!trackChanges)
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }

            var uow = new UnitOfWork(context, _serviceProvider, _mediator);
            return uow;
        }
    }
}
