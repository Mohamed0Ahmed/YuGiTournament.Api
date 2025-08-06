namespace YuGiTournament.Api.DTOs
{
    public class PaginatedShutoutResultDto
    {
        public IEnumerable<ShutoutResultDto> Shutouts { get; set; } = [];
        public int TotalShutouts { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public string FilterSummary { get; set; } = string.Empty;
    }
}