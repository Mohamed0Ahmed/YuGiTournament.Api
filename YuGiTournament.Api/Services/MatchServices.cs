using YuGiTournament.Api.Models;
using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class MatchService : IMatchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MatchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<object>> GetAllMatchesAsync()
        {
            return await _unitOfWork.GetRepository<Match>()
                .GetAll()
                .Select(m => new
                {
                    m.MatchId,
                    m.Score1,
                    m.Score2,
                    m.IsCompleted,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    m.Player1Id,
                    m.Player2Id,
                })
                .ToListAsync();
        }

        public async Task<object?> GetMatchByIdAsync(int matchId)
        {
            return await _unitOfWork.GetRepository<Match>()
                .GetAll()
                .Where(m => m.MatchId == matchId)
                .Select(m => new
                {
                    m.MatchId,
                    m.Score1,
                    m.Score2,
                    m.IsCompleted,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName
                })
                .FirstOrDefaultAsync();
        }


        public async Task<ApiResponse> ResetMatchByIdAsync(int matchId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var match = await _unitOfWork.GetRepository<Match>()
                    .GetAll()
                    .Include(m => m.Player1)
                    .Include(m => m.Player2)
                    .Include(m => m.Rounds)
                    .FirstOrDefaultAsync(m => m.MatchId == matchId);

                if (match == null)
                    return new ApiResponse(false, "الماتش ده مش موجود");

                if (!match.Rounds.Any())
                    return new ApiResponse(false, "الماتش لسه متلعبش");

                var player1 = match.Player1;
                var player2 = match.Player2;

                if (player1 == null || player2 == null)
                    return new ApiResponse(false, "اللاعبين غير موجودين");

                foreach (var round in match.Rounds)
                {
                    switch (round.WinnerId)
                    {
                        case var winnerId when winnerId == player1.PlayerId:
                            player1.Wins--;
                            player1.Points--;
                            player2.Losses--;
                            break;
                        case var winnerId when winnerId == player2.PlayerId:
                            player2.Wins--;
                            player2.Points--;
                            player1.Losses--;
                            break;
                        case null when round.IsDraw:
                            player1.Draws--;
                            player2.Draws--;
                            player1.Points -= 0.5;
                            player2.Points -= 0.5;
                            break;
                    }
                }

                _unitOfWork.GetRepository<MatchRound>().DeleteRange(match.Rounds);
                match.Score1 = 0;
                match.Score2 = 0;
                match.IsCompleted = false;

                player1.UpdateStats();
                player2.UpdateStats();

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new ApiResponse(true, "تم إعادة تعيين الماتش من البداية.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return new ApiResponse(false, $"حصل خطأ أثناء إعادة تعيين الماتش: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateMatchResultAsync(int matchId, MatchResultDto resultDto)
        {
            using var transaction = await _unitOfWork.GetDbContext().Database.BeginTransactionAsync();
            try
            {
                var match = await _unitOfWork.GetRepository<Match>().Find(match => match.MatchId == matchId).FirstOrDefaultAsync();
                if (match == null)
                    return new ApiResponse(false, "No Match Here");

                var player1 = await _unitOfWork.GetRepository<Player>().Find(player => player.PlayerId == match.Player1Id).FirstOrDefaultAsync();
                var player2 = await _unitOfWork.GetRepository<Player>().Find(player => player.PlayerId == match.Player2Id).FirstOrDefaultAsync();

                if (player1 == null || player2 == null)
                    return new ApiResponse(false, "All Players Must Be There");

                int matchCount = await _unitOfWork.GetRepository<MatchRound>().GetAll().CountAsync(mr => mr.MatchId == matchId);
                if (matchCount >= 5)
                    return new ApiResponse(false, "خلاص بقي هم لعبوا ال 5 ماتشات والله");

                var newRound = new MatchRound { MatchId = matchId };
                string responseMessage;

                if (resultDto.WinnerId == null)
                {
                    newRound.IsDraw = true;
                    player1.Draws += 1;
                    player2.Draws += 1;
                    match.Score1 += 0.5;
                    match.Score2 += 0.5;
                    responseMessage = "تم اضافة نصف نقطة لكلا اللاعبين";
                }
                else if (resultDto.WinnerId == match.Player1Id || resultDto.WinnerId == match.Player2Id)
                {
                    var winner = resultDto.WinnerId == match.Player1Id ? player1 : player2;
                    var loser = resultDto.WinnerId == match.Player1Id ? player2 : player1;

                    newRound.WinnerId = winner!.PlayerId;
                    winner.Wins += 1;
                    loser.Losses += 1;

                    if (winner.PlayerId == match.Player1Id)
                        match.Score1 += 1;
                    else
                        match.Score2 += 1;

                    responseMessage = $"تم اضافة نقطة للاعب : {winner.FullName}";
                }
                else
                {
                    return new ApiResponse(false, "Invalid winnerId. The winner must be one of the match players");
                }

                await _unitOfWork.GetRepository<MatchRound>().AddAsync(newRound);
                match.IsCompleted = matchCount + 1 == 5;
                player1.UpdateStats();
                player2.UpdateStats();

                _unitOfWork.GetRepository<Player>().Update(player1);
                _unitOfWork.GetRepository<Player>().Update(player2);
                _unitOfWork.GetRepository<Match>().Update(match);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse(true, responseMessage);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse(false, $"Error updating match: {ex.Message}");
            }
        }

    }
}