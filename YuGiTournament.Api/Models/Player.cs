namespace YuGiTournament.Api.Models
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public double Points { get; set; }

        public int MatchesPlayed { get; set; }

        public double WinRate { get; set; }

        public ICollection<Match> Matches { get; set; } = new List<Match>();

        public void UpdateStats()
        {
            MatchesPlayed = Wins + Losses + Draws;
            WinRate = MatchesPlayed > 0 ? (double)Wins / MatchesPlayed * 100 : 0;
        }
    }
}
