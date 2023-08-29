using Groove.SP.Application.Authorization;
using Groove.SP.Application.QuickTrack.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuickTracksController : ControllerBase
    {
        private readonly IQuickTrackService _quickSearchService;

        public QuickTracksController(IQuickTrackService quickSearchService)
        {
            _quickSearchService = quickSearchService;
        }

        [HttpGet("{searchTerm}/track-option")]
        [AppAuthorize]
        public async Task<IActionResult> GetQuickTrackOptionAsync(string searchTerm, string affiliates = "", string supplierCustomerRelationships = "")
        {
            var viewModels = await _quickSearchService.GetQuickTrackOptionAsync(
                searchTerm,
                CurrentUser.IsInternal,
                CurrentUser.OrganizationId,
                affiliates,
                supplierCustomerRelationships);

            return new JsonResult(viewModels);
        }
    }
}