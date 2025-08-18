using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Models
{
    public class Player : DeletedEntity
    {
        public int PlayerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public double Points { get; set; }
        public int MatchesPlayed { get; set; }
        public int Rank { get; set; }

        public double WinRate { get; set; }
        public int? GroupNumber { get; set; }



        public void UpdateStats(SystemOfLeague system = SystemOfLeague.Points)
        {
            MatchesPlayed = Wins + Losses + Draws;
            WinRate = MatchesPlayed > 0 ? (double)Wins / MatchesPlayed * 100 : 0;

            if (system == SystemOfLeague.Classic)
            {
                Points = (Wins * 3) + Draws;  // Classic: فوز = 3، تعادل = 1
            }
            else
            {
                Points = Wins + ((double)Draws / 2);  // Points: فوز = 1، تعادل = 0.5
            }
        }
    }
}