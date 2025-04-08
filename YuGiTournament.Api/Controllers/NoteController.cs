using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }





        [Authorize(Roles = "admin")]
        [HttpPost("send")]
        public async Task<IActionResult> WriteNote([FromBody] NoteDto note)
        {
            var response = await _noteService.WriteNote(note.Content);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("inbox")]
        public async Task<IActionResult> GetNotes()
        {


            var (response, messages) = await _noteService.GetNotesAsync();
            return Ok(new { response.Success, response.Message, Messages = messages });
        }





        [Authorize(Roles = "Admin")]
        [HttpPost("mark/{noteId}")]
        public async Task<IActionResult> HideNoteToggle(int NoteId, [FromBody] HideNoteDto note)
        {
            var response = await _noteService.HideNoteAsync(NoteId, note.Marked);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{noteId}")]
        public async Task<IActionResult> DeleteNote(int NoteId, [FromBody] HideNoteDto note)
        {
            var response = await _noteService.SoftDelete(NoteId, note.Marked);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }


    }
}
