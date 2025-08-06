namespace YuGiTournament.Api.Models
{
    public class FriendlyMatch
    {
        public int MatchId { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public DateTime PlayedOn { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public FriendlyPlayer Player1 { get; set; } = null!;
        public FriendlyPlayer Player2 { get; set; } = null!;
    }
} 