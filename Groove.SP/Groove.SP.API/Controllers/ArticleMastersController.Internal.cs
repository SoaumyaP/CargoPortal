using Groove.SP.API.Filters;
using Groove.SP.Application.ArticleMaster.Services.Interfaces;
using Groove.SP.Application.Authorization;
using Groove.SP.Core.Data;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers;

[Route("api/articlemasters/internal")]
[ApiController]
public class ArticleMastersInternalController : ControllerBase
{
    private readonly IArticleMasterService _articleMasterService;
    private readonly ICSFEApiClient _csfeApiClient;
    public ArticleMastersInternalController(IArticleMasterService articleMasterService, ICSFEApiClient csfeApiClient)
    {
        _articleMasterService = articleMasterService;
        _csfeApiClient = csfeApiClient;
    }

    [HttpGet]
    [Route("search")]
    [AppAuthorize(AppPermissions.Organization_ArticleMaster_List)]
    public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request, string affiliates)
    {
        var viewModels = await _articleMasterService.ListAsync(request, CurrentUser.IsInternal, CurrentUser.OrganizationId, affiliates);
        return new JsonResult(viewModels);
    }

    [HttpGet]
    [Route("{id}")]
    [AppAuthorize(AppPermissions.Organization_ArticleMaster_Detail)]
    public async Task<IActionResult> GetAsync(long id)
    {
        var viewModel = await _articleMasterService.GetByIdAsync(id);
        if (viewModel != null)
        {
            var organization = await _csfeApiClient.GetOrganizationsByCodeAsync(viewModel.CompanyCode);
            if (organization != null)
            {
                viewModel.CompanyName = organization.Name;
            }
        }
        return new JsonResult(viewModel);
    }
}

