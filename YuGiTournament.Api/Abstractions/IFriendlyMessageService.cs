using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Abstractions
{
    public interface IFriendlyMessageService
    {
        Task<ApiResponse> SendMessageToAdminAsync(string playerId, string content);
        Task<(ApiResponse Response, List<FriendlyMessage> Messages)> GetInboxAsync();
        Task<(ApiResponse Response, List<FriendlyMessage> Messages)> GetReadMessagesAsync();
        Task<(ApiResponse Response, List<FriendlyMessage> Messages)> GetUnreadMessagesAsync();
        Task<ApiResponse> MarkAsync(int messageId, bool marked);
        Task<ApiResponse> SoftDelete(int messageId, bool marked);
        Task<(ApiResponse Response, List<FriendlyMessage> Messages)> GetPlayerMessagesAsync(string playerId);
        Task<ApiResponse> SendAdminReplyAsync(string adminId, string playerId, string content);
    }
}