namespace YuGiTournament.Api.Models
{
    public class ShutoutResult
    {
        public int ShutoutId { get; set; }
        public int MatchId { get; set; }
        public int WinnerId { get; set; }
        public int LoserId { get; set; }
        public DateTime AchievedOn { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public FriendlyMatch Match { get; set; } = null!;
        public FriendlyPlayer Winner { get; set; } = null!;
        public FriendlyPlayer Loser { get; set; } = null!;
    }
}