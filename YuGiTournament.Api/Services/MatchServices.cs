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

            var player1 = await _context.Players.FindAsync(match.Player1Id);
            var player2 = await _context.Players.FindAsync(match.Player2Id);

            if (player1 == null || player2 == null)
                return new ApiResponse("فيه واحد من الرجالة دي مش موجود");

            if (match.Score1 > match.Score2)
            {
                player1.Wins -= 1;
                player2.Losses -= 1;
                player1.Points -= 1;
            }
            else if (match.Score1 < match.Score2)
            {
                player2.Wins -= 1;
                player1.Losses -= 1;
                player2.Points -= 1;
            }
            else if (match.Score1 == match.Score2)
            {
                player1.Points -= 0.5;
                player2.Points -= 0.5;
                player1.Draws -= 1;
                player2.Draws -= 1;
            }

            match.Score1 = 0;
            match.Score2 = 0;
            match.IsCompleted = false;

            await _context.SaveChangesAsync();
            return new ApiResponse("تم إعادة تعيين الماتش من البداية.");
        }


        public async Task<Match> CreateMatchAsync(int player1Id, int player2Id)
        {
            var player1 = await _context.Players.FindAsync(player1Id);
            var player2 = await _context.Players.FindAsync(player2Id);

            if (player1 == null || player2 == null)
                throw new Exception("One or both players not found");

            var match = new Match
            {
                Player1Id = player1Id,
                Player2Id = player2Id,
                Score1 = 0,
                Score2 = 0,
                IsCompleted = false
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
            return match;
        }


        public async Task<ApiResponse> UpdateMatchResultAsync(int matchId, MatchResultDto resultDto)
        {
            var match = await _context.Matches.FirstOrDefaultAsync(m => m.MatchId == matchId);

            if (match == null)
                return new ApiResponse("No Match Here");

            var player1 = await _context.Players.FindAsync(match.Player1Id);
            var player2 = await _context.Players.FindAsync(match.Player2Id);

            if (player1 == null || player2 == null)
                return new ApiResponse("All Players Must Be There");

            int matchCount = await _context.Matches.CountAsync(m =>
                (m.Player1Id == match.Player1Id && m.Player2Id == match.Player2Id) ||
                (m.Player1Id == match.Player2Id && m.Player2Id == match.Player1Id));

            if (matchCount >= 5)
                return new ApiResponse("خلاص بقي هم لعبوا ال 5 ماتشات والله");

            if (resultDto.WinnerId == null)
            {
                HandleDraw(match, player1, player2);
                await _context.SaveChangesAsync();
                return new ApiResponse("تم اضافة نصف نقطة لكلا اللاعبين");
            }

            if (resultDto.WinnerId != match.Player1Id && resultDto.WinnerId != match.Player2Id)
                return new ApiResponse("Invalid winnerId. The winner must be one of the match players");

            var winner = resultDto.WinnerId == match.Player1Id ? player1 : player2;
            var loser = resultDto.WinnerId == match.Player1Id ? player2 : player1;

            winner!.Points += 1;
            winner.Wins += 1;
            loser.Losses += 1;

            if (resultDto.WinnerId == match.Player1Id)
                match.Score1 += 1;
            else
                match.Score2 += 1;

            match.IsCompleted = (match.Score1 + match.Score2) >= 5;

            await _context.SaveChangesAsync();
            return new ApiResponse($"تم تحديث النتيجة واضافة نقطة للاعب {winner!.FullName}");
        }






        //**********************
        private void HandleDraw(Match match, Player player1, Player player2)
        {
            match.Score1 += 0.5;
            match.Score2 += 0.5;

            player1.Points += 0.5;
            player1.Draws += 1;

            player2.Points += 0.5;
            player2.Draws += 1;
        }

    }
}
