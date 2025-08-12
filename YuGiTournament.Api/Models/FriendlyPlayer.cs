namespace YuGiTournament.Api.Models
{
    public class FriendlyPlayer
    {
        public int PlayerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Multi tournaments aggregate statistics (global)
        public int MultiParticipations { get; set; } = 0;
        public int MultiTitlesWon { get; set; } = 0;

        // Navigation Properties
        public ICollection<FriendlyMatch> Player1Matches { get; set; } = [];
        public ICollection<FriendlyMatch> Player2Matches { get; set; } = [];
    }
}