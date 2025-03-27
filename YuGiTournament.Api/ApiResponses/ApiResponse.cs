namespace YuGiTournament.Api.ApiResponses
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public ApiResponse() { }

        public ApiResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}