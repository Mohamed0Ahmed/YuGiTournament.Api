namespace YuGiTournament.Api.ApiResponses
{
    public class ApiResponse(bool success, string message)
    {
        public bool Success { get; set; } = success;
        public string? Message { get; set; } = message;
    }
}