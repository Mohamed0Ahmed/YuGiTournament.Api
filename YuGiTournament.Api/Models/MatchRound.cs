namespace YuGiTournament.Api.Models
{
    public class MatchRound : DeletedEntity
    {
        public int MatchRoundId { get; set; }
        public int MatchId { get; set; }
        public int? WinnerId { get; set; }
        public bool IsDraw { get; set; }

        public Match Match { get; set; } = null!;
    }
}