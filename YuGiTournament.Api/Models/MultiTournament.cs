namespace YuGiTournament.Api.Models
{
    public enum TournamentStatus
    {
        Created = 0,    // إضافة فرق ولاعبين
        Started = 1,    // بعد توليد المباريات + بدء اللعب
        Finished = 2    // انتهاء البطولة
    }

    public class MultiTournament
    {
        public int MultiTournamentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public SystemOfLeague SystemOfScoring { get; set; } = SystemOfLeague.Points;
        public int TeamCount { get; set; }
        public int PlayersPerTeam { get; set; }
        public TournamentStatus Status { get; set; } = TournamentStatus.Created;
        public bool IsActive { get; set; } = false; // بطولة واحدة نشطة فقط
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? StartedOn { get; set; }
        public DateTime? FinishedOn { get; set; }
        public int? ChampionTeamId { get; set; }

        // Navigation Properties
        public ICollection<MultiTeam> Teams { get; set; } = [];
        public ICollection<MultiMatch> Matches { get; set; } = [];
        public MultiTeam? ChampionTeam { get; set; }
    }
}


