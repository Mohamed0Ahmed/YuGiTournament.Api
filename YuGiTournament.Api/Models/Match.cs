namespace YuGiTournament.Api.Models
{
    public class Match
    {
        public int MatchId { get; set; }  
        public int Player1Id { get; set; }  
        public int Player2Id { get; set; }  
        public int Score1 { get; set; }
        public int Score2 { get; set; }
        public bool IsCompleted { get; set; }

        public virtual Player Player1 { get; set; } = null!;
        public virtual Player Player2 { get; set; } = null!;
    }
}
