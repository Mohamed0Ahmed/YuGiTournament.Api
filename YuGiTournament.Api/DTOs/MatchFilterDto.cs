namespace YuGiTournament.Api.DTOs
{
    public class MatchFilterDto
    {
        public int? PlayerId { get; set; }
        public List<int>? OpponentIds { get; set; }
        public DateFilter DateFilter { get; set; } = DateFilter.AllTime;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public enum DateFilter
    {
        Today = 1,
        Last3Days = 2,
        LastWeek = 3,
        LastMonth = 4,
        AllTime = 5
    }
}