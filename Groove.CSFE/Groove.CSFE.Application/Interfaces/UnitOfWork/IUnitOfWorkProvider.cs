// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUnitOfWorkProvider.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   Unit Of Work Provider Interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Groove.CSFE.Application.Interfaces.UnitOfWork
{
    public interface IUnitOfWorkProvider
    {
        IUnitOfWork CreateUnitOfWork(bool trackChanges = true, bool enableLogging = false);

        IUnitOfWork CreateUnitOfWorkForBackgroundJob(bool trackChanges = true, bool enableLogging = false);
    }
}
