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
            if (notes.Count == 0)
                return (new ApiResponse(false, "No Notes Here"), notes);



            return (new ApiResponse(true, "Note Returned Successfully"), notes);

        }

        public async Task<ApiResponse> ToggleHideNoteAsync(int noteId, bool marked)
        {

            var note = await _unitOfWork.GetRepository<Note>()
                                        .Find(n => n.Id == noteId)
                                        .FirstOrDefaultAsync();

            if (note == null)
                return new ApiResponse(false, "مفيش ملاحظات بالاي دي ده");

            note.IsHidden = !note.IsHidden;
            _unitOfWork.GetRepository<Note>().Update(note);
            await _unitOfWork.SaveChangesAsync();

            var message = note.IsHidden ? "تم اخفاء الملاحظة" : "تم اظهار الملاحظة";
            return new ApiResponse(true, message);

        }

        public async Task<ApiResponse> SoftDelete(int noteId)
        {
            var note = await _unitOfWork.GetRepository<Note>()
                                       .Find(n => n.Id == noteId)
                                       .FirstOrDefaultAsync();

            if (note == null)
                return new ApiResponse(false, "مفيش ملاحظات بالاي دي ده");

            if (note.IsDeleted == true)
                return new ApiResponse(false, "الملاحظة تم مسحها بالفعل");


            note.IsDeleted = true;
            _unitOfWork.GetRepository<Note>().Update(note);
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
