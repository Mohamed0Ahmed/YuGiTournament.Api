using YuGiTournament.Api.Data;
using YuGiTournament.Api.Models;
using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
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



        public async Task DeleteMatchAsync(int matchId)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null) return;

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            await ResetPlayerStatsAsync(match);
        }

        private async Task UpdatePlayerStatsAsync(Match match)
        {
            var player1 = await _context.Players.FindAsync(match.Player1Id);
            var player2 = await _context.Players.FindAsync(match.Player2Id);

            if (player1 == null || player2 == null) return;

            if (match.Score1 > match.Score2)
            {
                player1.Points += 1;
                player1.Wins += 1;
            }
            else if (match.Score2 > match.Score1)
            {
                player2.Points += 1;
                player2.Wins += 1;
            }
            else
            {
                player1.Points += 0.5;
                player2.Points += 0.5;
                player1.Draws += 1;
                player2.Draws += 1;
            }

            await _context.SaveChangesAsync();
        }

        private async Task ResetPlayerStatsAsync(Match match)
        {
            var player1 = await _context.Players.FindAsync(match.Player1Id);
            var player2 = await _context.Players.FindAsync(match.Player2Id);

            if (player1 == null || player2 == null) return;

            if (match.Score1 > match.Score2)
            {
                player1.Points -= 1;
                player1.Wins -= 1;
            }
            else if (match.Score2 > match.Score1)
            {
                player2.Points -= 1;
                player2.Wins -= 1;
            }
            else
            {
                player1.Points -= 0.5;
                player2.Points -= 0.5;
                player1.Draws -= 1;
                player2.Draws -= 1;
            }

            await _context.SaveChangesAsync();
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



        public   async Task<ApiResponse> UpdateMatchResultAsync(int matchId, MatchResultDto resultDto)
        {
            var match = await _context.Matches.FirstOrDefaultAsync(m => m.MatchId == matchId);

            if (match == null)
                return new ApiResponse("No Match Here");

            if (match.IsCompleted)
                return new ApiResponse("خلاص بقي هم لعبوا ال 5 ماتشات والله ");
            


            if (resultDto.WinnerId != null && resultDto.WinnerId != match.Player1Id && resultDto.WinnerId != match.Player2Id)
                return new ApiResponse("Invalid winnerId. The winner must be one of the match players");

            var player1 = await _context.Players.FindAsync(match.Player1Id);
            var player2 = await _context.Players.FindAsync(match.Player2Id);
            var winner = resultDto.WinnerId == match.Player1Id ? player1 : player2;

            if (player1 == null || player2 == null)
                return new ApiResponse("All Players Must Be There");

            if (resultDto.WinnerId == null)
            {
                match.Score1 += 1;
                match.Score2 += 1;

                player1.Points += 0.5;
                player1.Draws += 1;

                player2.Points += 0.5;
                player2.Draws += 1;
                return new ApiResponse("تم اضافة نصف نقطة لكلا اللاعبين");
            }
            else
            {
                //var winne = resultDto.WinnerId == match.Player1Id ? player1 : player2;
                var loser = resultDto.WinnerId == match.Player1Id ? player2 : player1;

                winner.Points += 1;
                winner.Wins += 1;
                loser.Losses += 1;

                if (resultDto.WinnerId == match.Player1Id)
                    match.Score1 += 1;
                else
                    match.Score2 += 1;


            }

            if (match.Player1.Points + match.Player2.Points == 5)
                match.IsCompleted = true;

            else
                match.IsCompleted = false;


            await _context.SaveChangesAsync();

            return new ApiResponse($"تم تحديث النتيجة واضافة نقطة للاعب {winner!.FullName}");
        }


    }
}
