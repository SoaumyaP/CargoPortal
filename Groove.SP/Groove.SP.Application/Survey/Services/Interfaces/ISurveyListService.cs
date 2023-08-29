using Groove.SP.Application.Common;
using Groove.SP.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.Survey.Services.Interfaces
{
    public interface ISurveyListService
    {
        Task<DataSourceResult> GetListAsync(DataSourceRequest request, IdentityInfo currentUser);
    }
}
