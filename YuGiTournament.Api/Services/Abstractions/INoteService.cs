using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface INoteService
    {

        Task<ApiResponse> WriteNote( string content);
        Task<(ApiResponse Response, List<Note> Notes)> GetNotesAsync();
        Task<ApiResponse> ToggleHideNoteAsync(int noteId);
        Task<ApiResponse> SoftDelete(int noteId);

    }
}
