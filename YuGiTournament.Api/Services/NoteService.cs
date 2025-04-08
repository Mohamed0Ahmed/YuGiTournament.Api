using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class NoteService : INoteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NoteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(ApiResponse Response, List<Note> Notes)> GetNotesAsync()
        {

            var notes = await _unitOfWork.GetRepository<Note>().GetAll().Where(n => n.IsDeleted != true).ToListAsync();

            return (new ApiResponse(true , "Note Returned Successfully"), notes);

        }

        public Task<ApiResponse> HideNoteAsync(int messageId, bool marked)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> SoftDelete(int messageId, bool marked)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> WriteNote(string content)
        {
            throw new NotImplementedException();
        }
    }
}
