namespace YuGiTournament.Api.Models
{
    public class MultiTeam
    {
        public int MultiTeamId { get; set; }
        public int MultiTournamentId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string PlayerIds { get; set; } = string.Empty; // JSON array: "[1,2,3]"
        public double TotalPoints { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public MultiTournament? Tournament { get; set; }
        public ICollection<MultiMatch> HomeMatches { get; set; } = []; // Team1 matches
        public ICollection<MultiMatch> AwayMatches { get; set; } = []; // Team2 matches
    }
}


