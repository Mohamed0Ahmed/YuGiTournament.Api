namespace YuGiTournament.Api.DTOs
{
    public class FriendlyMatchHistoryDto
    {
        public int MatchId { get; set; }
        public DateTime PlayedOn { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public string Winner { get; set; } = string.Empty;
        public string Player1Name { get; set; } = string.Empty;
        public string Player2Name { get; set; } = string.Empty;
    }
} 