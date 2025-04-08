﻿using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IMessageService
    {
        Task<ApiResponse> SendMessageToAdminAsync(string playerId, string content);
        Task<(ApiResponse Response, List<Message> Messages)> GetInboxAsync(string adminId);
        Task<(ApiResponse Response, List<Message> Messages)> GetReadMessagesAsync(string adminId);
        Task<(ApiResponse Response, List<Message> Messages)> GetUnreadMessagesAsync(string adminId);
        Task<ApiResponse> MarkAsync(int messageId, bool marked);
        Task<ApiResponse> SoftDelete(int messageId, bool marked);
    }
}