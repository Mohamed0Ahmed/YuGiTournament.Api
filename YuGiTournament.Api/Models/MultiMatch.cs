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

        // Classic scoring (allowed values: 3/0, 0/3, 1/1)
        public int? Score1 { get; set; }
        public int? Score2 { get; set; }

        // Points scoring: sum of rounds per player
        public double? TotalPoints1 { get; set; }
        public double? TotalPoints2 { get; set; }

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedOn { get; set; }

        // Navigation Properties
        public MultiTournament? Tournament { get; set; }
        public MultiTeam? Team1 { get; set; }
        public MultiTeam? Team2 { get; set; }
        public FriendlyPlayer? Player1 { get; set; }
        public FriendlyPlayer? Player2 { get; set; }
    }
}


