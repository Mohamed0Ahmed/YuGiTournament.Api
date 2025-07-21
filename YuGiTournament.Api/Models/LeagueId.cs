﻿namespace YuGiTournament.Api.Models
{
    public class LeagueId
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public LeagueType TypeOfLeague { get; set; }
        public bool IsFinished { get; set; }= false;
        public bool IsDeleted { get; set; }= false;
        public SystemOfLeague SystemOfLeague { get; set; } = SystemOfLeague.Matches;
    }


    //*********************


    public enum LeagueType
    {
        Single = 0,
        Multi = 1
    }

    public enum SystemOfLeague
    {
        Points = 0,
        Matches = 1
    }
}