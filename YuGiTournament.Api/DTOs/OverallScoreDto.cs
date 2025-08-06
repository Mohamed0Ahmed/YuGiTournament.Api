namespace YuGiTournament.Api.DTOs
{
    public class OverallScoreDto
    {
        public string Player1Name { get; set; } = string.Empty;
        public string Player2Name { get; set; } = string.Empty;
        public int Player1TotalScore { get; set; }
        public int Player2TotalScore { get; set; }
        public int TotalMatches { get; set; }
        public string LeadingPlayer { get; set; } = string.Empty;
        public int ScoreDifference { get; set; }

        // Win/Loss Statistics
        public int Player1Wins { get; set; }
        public int Player1Losses { get; set; }
        public int Player1Draws { get; set; }
        public int Player2Wins { get; set; }
        public int Player2Losses { get; set; }
        public int Player2Draws { get; set; }

        // Win Rates
        public double Player1WinRate { get; set; }
        public double Player2WinRate { get; set; }

        // Match History
        public IEnumerable<FriendlyMatchHistoryDto> MatchHistory { get; set; } = [];
    }
}