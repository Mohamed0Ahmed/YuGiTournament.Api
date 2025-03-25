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

        public virtual List<Match> Matches { get; set; } = [];
    }
}
