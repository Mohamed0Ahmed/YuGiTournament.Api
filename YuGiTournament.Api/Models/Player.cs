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
        public int MatchesPlayed => Wins + Losses + Draws;
        public double WinRate => MatchesPlayed > 0 ? (double)Wins / MatchesPlayed * 100 : 0;

        public  List<Match> Matches { get; set; } = [];
    }
}
