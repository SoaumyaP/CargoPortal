// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRepositoryInjection.cs"  company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   Repository Injection Interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace Groove.SP.Persistence.Repositories.Base
{
    public interface IRepositoryInjection
    {
        IRepositoryInjection SetContext(DbContext context);
    }
}
