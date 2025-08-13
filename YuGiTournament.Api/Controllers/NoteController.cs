using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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



        [Authorize(Roles = "Admin")]
        [HttpPost("write")]
        public async Task<IActionResult> WriteNote([FromBody] NoteDto note)
        {
            var response = await _noteService.WriteNote(note.Content);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }


        [HttpGet("notes")]
        public async Task<IActionResult> GetNotes()
        {
            var (response, notes) = await _noteService.GetNotesAsync();
            return Ok(new { response.Success, response.Message, Notes = notes });
        }




        [Authorize(Roles = "Admin")]
        [HttpPost("hide/{noteId}")]
        public async Task<IActionResult> HideNoteToggle(int NoteId)
        {
            var response = await _noteService.ToggleHideNoteAsync(NoteId);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }




        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{noteId}")]
        public async Task<IActionResult> DeleteNote(int NoteId)
        {
            var response = await _noteService.SoftDelete(NoteId);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }


    }
}
