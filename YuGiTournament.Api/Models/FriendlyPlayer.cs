namespace YuGiTournament.Api.Models
{
    public class FriendlyPlayer
    {
        public int PlayerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public ICollection<FriendlyMatch> Player1Matches { get; set; } = [];
        public ICollection<FriendlyMatch> Player2Matches { get; set; } = [];
    }
}