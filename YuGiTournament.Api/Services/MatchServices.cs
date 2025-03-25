using YuGiTournament.Api.Data;
using YuGiTournament.Api.Models;
using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.ApiResponses;

namespace YuGiTournament.Api.Services
{
    public class MatchService : IMatchService
    {
        private readonly ApplicationDbContext _context;

        public MatchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetAllMatchesAsync()
        {
            return await _context.Matches
                .Select(m => new
                {
                    m.MatchId,
                    m.Score1,
                    m.Score2,
                    m.IsCompleted,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName
                })
                .ToListAsync();
        }

        public async Task<object?> GetMatchByIdAsync(int matchId)
        {
            return await _context.Matches
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
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null)
                return new ApiResponse("الماتش ده مش موجود");

            var matchRounds = await _context.MatchRounds.Where(mr => mr.MatchId == matchId).ToListAsync();
            if (!matchRounds.Any())
                return new ApiResponse("الماتش لسه متلعبش");

            var player1 = await _context.Players.FindAsync(match.Player1Id);
            var player2 = await _context.Players.FindAsync(match.Player2Id);

            foreach (var round in matchRounds)
            {
                if (round.WinnerId == player1?.PlayerId)
                {
                    player1!.Wins -= 1;
                    player1.Points -= 1;
                    player2!.Losses -= 1;
                }
                else if (round.WinnerId == player2?.PlayerId)
                {
                    player2!.Wins -= 1;
                    player2.Points -= 1;
                    player1!.Losses -= 1;
                }
                else if (round.IsDraw)
                {
                    player1!.Draws -= 1;
                    player2!.Draws -= 1;
                    player1.Points -= 0.5;
                    player2.Points -= 0.5;
                }
            }

            _context.MatchRounds.RemoveRange(matchRounds);
            match.Score1 = 0;
            match.Score2 = 0;
            match.IsCompleted = false;

            player1!.UpdateStats();
            player2!.UpdateStats();
            await _context.SaveChangesAsync();
            return new ApiResponse("تم إعادة تعيين الماتش من البداية.");
        }

        public async Task<ApiResponse> UpdateMatchResultAsync(int matchId, MatchResultDto resultDto)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null)
                return new ApiResponse("No Match Here");

            var player1 = await _context.Players.FindAsync(match.Player1Id);
            var player2 = await _context.Players.FindAsync(match.Player2Id);
            var winner = resultDto.WinnerId == match.Player1Id ? player1 : player2;
            if (player1 == null || player2 == null)
                return new ApiResponse("All Players Must Be There");

            int matchCount = await _context.MatchRounds.CountAsync(mr => mr.MatchId == matchId);
            if (matchCount >= 5)
                return new ApiResponse("خلاص بقي هم لعبوا ال 5 ماتشات والله");

            var newRound = new MatchRound { MatchId = matchId };
            if (resultDto.WinnerId == null)
            {
                newRound.IsDraw = true;
                player1.Points += 0.5;
                player2.Points += 0.5;
                player1.Draws += 1;
                player2.Draws += 1;
                match.Score1 += 0.5;
                match.Score2 += 0.5;
            }
            else if (resultDto.WinnerId == match.Player1Id || resultDto.WinnerId == match.Player2Id)
            {
               
                var loser = resultDto.WinnerId == match.Player1Id ? player2 : player1;

                newRound.WinnerId = winner!.PlayerId;
                winner.Points += 1;
                winner.Wins += 1;
                loser.Losses += 1;

                if (winner.PlayerId == match.Player1Id)
                    match.Score1 += 1;
                else
                    match.Score2 += 1;
            }
            else
            {
                return new ApiResponse("Invalid winnerId. The winner must be one of the match players");
            }

            _context.MatchRounds.Add(newRound);
            match.IsCompleted = await _context.MatchRounds.CountAsync(mr => mr.MatchId == matchId) >= 5;
            player1.UpdateStats();
            player2.UpdateStats();
            await _context.SaveChangesAsync();
            return new ApiResponse($"تم اضافة نقطة للاعب : {winner!.FullName}");
        }
    }
}
