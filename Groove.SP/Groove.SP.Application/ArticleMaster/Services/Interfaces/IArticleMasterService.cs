using Groove.SP.Application.ArticleMaster.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.ArticleMaster.Services.Interfaces;
public interface IArticleMasterService : IServiceBase<ArticleMasterModel, ArticleMasterViewModel>
{
    Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long organizationId, string affiliates = "");
    Task<ArticleMasterViewModel> GetByIdAsync(long id);
}