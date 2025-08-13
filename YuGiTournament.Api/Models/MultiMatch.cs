namespace YuGiTournament.Api.Models
{
    public class MultiMatch
    {
        public int MultiMatchId { get; set; }
        public int MultiTournamentId { get; set; }
        public int Team1Id { get; set; }
        public int Team2Id { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }

        // Match scores for both Classic and Points systems
        public double? Score1 { get; set; }
        public double? Score2 { get; set; }

        // Winner of the match (null for draw)
        public int? WinnerId { get; set; }

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedOn { get; set; }

        // Navigation Properties
        public MultiTournament? Tournament { get; set; }
        public MultiTeam? Team1 { get; set; }
        public MultiTeam? Team2 { get; set; }
        public FriendlyPlayer? Player1 { get; set; }
        public FriendlyPlayer? Player2 { get; set; }
        public FriendlyPlayer? Winner { get; set; }
    }
}


