using Groove.SP.Application.Authorization;
using Groove.SP.Application.Note.Services.Interfaces;
using Groove.SP.Application.Note.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class NotesController : ControllerBase
    {
        public readonly INoteService _noteService;

        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var result = await _noteService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]      
        public async Task<IActionResult> PostAsync([FromBody]CreateAndUpdateNoteViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _noteService.CreateAsync(model);
            return Ok(result);
        }


        [HttpPut("{id}")]       
        public async Task<IActionResult> PutAsync(long id, [FromBody]CreateAndUpdateNoteViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _noteService.UpdateAsync(id, model);
            return Ok(result);
        }

        [HttpDelete("{id}")]       
        public async Task<IActionResult> DeleteAsync(long id)
        {
            await _noteService.DeleteAsync(id);
            return Ok();
        }
    }
}
