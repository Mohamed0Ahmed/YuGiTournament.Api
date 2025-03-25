using YuGiTournament.Api.Data;
using YuGiTournament.Api.Models;
using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Services.Abstractions;

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


        public async Task AddMatchAsync(Match match)
        {
            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            await UpdatePlayerStatsAsync(match);
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

        public async Task<bool> UpdateMatchResultAsync(int matchId, int? winnerId)
        {
            var match = await _context.Matches.Include(m => m.Player1).Include(m => m.Player2)
                                              .FirstOrDefaultAsync(m => m.MatchId == matchId);

            if (match == null || match.IsCompleted)
                return false;

            if (winnerId == null) 
            {
                match.Score1++;
                match.Score2++;

                match.Player1.Points += 0.5;
                match.Player2.Points += 0.5;

                match.Player1.Draws++;
                match.Player2.Draws++;
            }
            else
            {
                var winner = winnerId == match.Player1Id ? match.Player1 : match.Player2;
                var loser = winnerId == match.Player1Id ? match.Player2 : match.Player1;

                winner.Points += 1;
                winner.Wins++;

                loser.Losses++;
            }

            //match.IsCompleted = true;
            await _context.SaveChangesAsync();
            return true;
        }



    }
}
