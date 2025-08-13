namespace YuGiTournament.Api.ApiResponses
{
    public class ApiResponse(bool success, string message)
    {
        public bool Success { get; set; } = success;
        public string? Message { get; set; } = message;
    }

    public class ApiResponse<T>(bool success, string message, T? data = default) : ApiResponse(success, message)
    {
        public T? Data { get; set; } = data;
    }
}