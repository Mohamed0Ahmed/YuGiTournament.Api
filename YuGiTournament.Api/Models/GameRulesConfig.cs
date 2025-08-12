namespace YuGiTournament.Api.Models
{
    public class GameRulesConfig
    {
        public int Id { get; set; }
        public int MaxRoundsPerMatch { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}


