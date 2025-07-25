namespace YuGiTournament.Api.Models.ViewModels
{
    public class MatchViewModel
    {
        public int MatchId { get; set; }
        public double Score1 { get; set; }
        public double Score2 { get; set; }
        public bool IsCompleted { get; set; }
        public string? Player1Name { get; set; }
        public string? Player2Name { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public string TournamentStage { get; set; } = string.Empty;
    }
}
