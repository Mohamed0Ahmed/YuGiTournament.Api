namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IMessageService
    {
        Task SendMessageToAdminAsync(string playerId, string content);
        Task<List<object>> GetInboxAsync(string adminId);
        Task MarkAsReadAsync(int messageId);
    }
}