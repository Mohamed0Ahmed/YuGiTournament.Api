namespace YuGiTournament.Api.Models.ViewModels
{
    public class LeagueWithMatchesViewModel
    {
        public int LeagueId { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public string LeagueDescription { get; set; } = string.Empty;
        public string LeagueType { get; set; }
        public bool IsFinished { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<MatchViewModel> Matches { get; set; } = []; 
    }
}
