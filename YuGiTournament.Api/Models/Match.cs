﻿namespace YuGiTournament.Api.Models
{
    public class Match : DeletedEntity
    {
        public int MatchId { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public double Score1 { get; set; }
        public double Score2 { get; set; }
        public bool IsCompleted { get; set; }
        public TournamentStage Stage { get; set; }

        public Player Player1 { get; set; } = null!;
        public Player Player2 { get; set; } = null!;

        public List<MatchRound> Rounds { get; set; } = [];
    }

    public enum TournamentStage
    {
        League,         // 0 - القيمة الافتراضية
        GroupStage,     // 1
        QuarterFinals,  // 2
        SemiFinals,     // 3
        Final           // 4
    }
}