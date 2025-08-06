namespace YuGiTournament.Api.DTOs
{
    public class ShutoutResultDto
    {
        public int ShutoutId { get; set; }
        public int MatchId { get; set; }
        public string WinnerName { get; set; } = string.Empty;
        public string LoserName { get; set; } = string.Empty;
        public DateTime AchievedOn { get; set; }
        public int WinnerScore { get; set; } = 5;
        public int LoserScore { get; set; } = 0;
        public string MatchDetails { get; set; } = string.Empty;
    }
}