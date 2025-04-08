using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface INoteService
    {

        Task<ApiResponse> WriteNote( string content);
        Task<(ApiResponse Response, List<Note> Notes)> GetNotesAsync();
        Task<ApiResponse> HideNoteAsync(int messageId, bool marked);
        Task<ApiResponse> SoftDelete(int messageId, bool marked);

    }
}
