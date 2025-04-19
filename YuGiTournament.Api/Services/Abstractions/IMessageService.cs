using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IMessageService
    {
        Task<ApiResponse> SendMessageToAdminAsync(string playerId, string content);
        Task<(ApiResponse Response, List<Message> Messages)> GetInboxAsync();
        Task<(ApiResponse Response, List<Message> Messages)> GetReadMessagesAsync();
        Task<(ApiResponse Response, List<Message> Messages)> GetUnreadMessagesAsync();
        Task<ApiResponse> MarkAsync(int messageId, bool marked);
        Task<ApiResponse> SoftDelete(int messageId, bool marked);
        Task<(ApiResponse Response, List<Message> Messages)> GetPlayerMessagesAsync(string playerId);
        Task<ApiResponse> SendAdminReplyAsync(string adminId, string playerId, string content); 
    }
}