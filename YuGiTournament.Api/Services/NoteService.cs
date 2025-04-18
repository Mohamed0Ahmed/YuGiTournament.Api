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
        private const string NoteNotFoundMessage = "مفيش ملاحظات بالاي دي ده";
        private const string NoteAlreadyDeletedMessage = "الملاحظة تم مسحها بالفعل";

        public NoteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(ApiResponse Response, List<Note> Notes)> GetNotesAsync()
        {
            var noteRepo = _unitOfWork.GetRepository<Note>();
            var notesQuery = noteRepo.GetAll().Where(n => !n.IsDeleted);

            if (!await notesQuery.AnyAsync())
                return (new ApiResponse(false, "No Notes Here"), new List<Note>());

            var notes = await notesQuery.ToListAsync();
            return (new ApiResponse(true, "Note Returned Successfully"), notes);
        }

        public async Task<ApiResponse> ToggleHideNoteAsync(int noteId)
        {
            var noteRepo = _unitOfWork.GetRepository<Note>();

            var note = await noteRepo.Find(n => n.Id == noteId).FirstOrDefaultAsync();

            if (note == null)
                return new ApiResponse(false, NoteNotFoundMessage);

            note.IsHidden = !note.IsHidden;
            noteRepo.Update(note);
            await _unitOfWork.SaveChangesAsync();

            var message = note.IsHidden ? "تم اخفاء الملاحظة" : "تم اظهار الملاحظة";
            return new ApiResponse(true, message);
        }

        public async Task<ApiResponse> SoftDelete(int noteId)
        {
            var noteRepo = _unitOfWork.GetRepository<Note>();

            var note = await noteRepo.Find(n => n.Id == noteId).FirstOrDefaultAsync();

            if (note == null)
                return new ApiResponse(false, NoteNotFoundMessage);

            if (note.IsDeleted)
                return new ApiResponse(false, NoteAlreadyDeletedMessage);

            note.IsDeleted = true;
            noteRepo.Update(note);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "تم مسح الملاحظة");
        }

        public async Task<ApiResponse> WriteNote(string content)
        {
            var note = new Note
            {
                Content = content
            };

            await _unitOfWork.GetRepository<Note>().AddAsync(note);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "تم اضافة الملاحظة");
        }

   
    }
}
