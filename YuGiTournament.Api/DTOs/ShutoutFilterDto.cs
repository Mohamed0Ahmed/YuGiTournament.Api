namespace YuGiTournament.Api.DTOs
{
    public class ShutoutFilterDto
    {
        public int? PlayerId { get; set; }
        public List<int>? PlayerIds { get; set; }
        public ShutoutPlayerRole? PlayerRole { get; set; }
        public DateFilter DateFilter { get; set; } = DateFilter.AllTime;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public enum ShutoutPlayerRole
    {
        Any = 1,        // اللاعب سواء كسب أو خسر
        Winner = 2,     // اللاعب كان الفائز بس
        Loser = 3       // اللاعب كان الخاسر بس
    }
}