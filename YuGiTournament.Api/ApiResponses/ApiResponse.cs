namespace YuGiTournament.Api.ApiResponses
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public ApiResponse() { }

        public ApiResponse(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }
    }
}
