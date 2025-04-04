using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.DTOs;
using System.Numerics;
using Azure;

namespace YuGiTournament.Api.Services
{
    public class LeagueResetService : ILeagueResetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LeagueResetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LeagueResponse> GetCurrentLeague()
        {
            var league = await _unitOfWork.GetRepository<LeagueId>().Find(x => x.IsFinished == false).FirstOrDefaultAsync();

            if (league == null)
                return new LeagueResponse { Response = new ApiResponse(false, "لا يوجد دوري حاليا"), League = null };

            var response = new ApiResponse(true, "بيانات الدوري الحالي");
            return new LeagueResponse { Response = response, League = league };
        }


        public async Task<ApiResponse> ResetLeagueAsync(int leagueId)
        {
            var dbContext = _unitOfWork.GetDbContext();
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var league = _unitOfWork.GetRepository<LeagueId>().Find(x => x.Id == leagueId).FirstOrDefault();
                if (league == null)
                    return new ApiResponse(false, "الدوري ده مش موجود");

                if (league.IsFinished == true)
                    return new ApiResponse(false, "هذا الدوري منتهي بالفعل");
                else
                    league.IsFinished = true;

                _unitOfWork.GetRepository<LeagueId>().Update(league); 

                await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DELETE FROM Messages");
                await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Messages', RESEED, 0)");

                await dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE Matches SET IsDeleted = 1 WHERE LeagueNumber = {0}", leagueId);
                await dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE MatchRounds SET IsDeleted = 1 WHERE LeagueNumber = {0}", leagueId);
                await dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE Players SET IsDeleted = 1 WHERE LeagueNumber = {0}", leagueId);

                await _unitOfWork.SaveChangesAsync(); 
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return new ApiResponse(false, "حدث خطأ اثناء انهاء الدوري");
            }

            return new ApiResponse(true, "تم انهاء الدوري");
        }


        public async Task<ApiResponse> StartLeagueAsync(StartLeagueDto newLeague)
        {
            var existLeague = await _unitOfWork.GetRepository<LeagueId>()
                .Find(x => x.IsFinished == false)
                .FirstOrDefaultAsync();
            if (existLeague != null)
                return new ApiResponse(false, $"هناك دوري حالي بالفعل");

            var league = new LeagueId
            {
                Description = newLeague.Description,
                Name = newLeague.Name,
                TypeOfLeague = newLeague.TypeOfLeague,
                IsFinished = false
            };

            var result = await _unitOfWork.GetRepository<LeagueId>().AddAsync(league);
            if (!result)
                return new ApiResponse(false, $"حدث خطأ اثناء حفظ الدوري");

            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, $"تم انشاء الدوري بنجاح");
        }
    }
}

