using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Data;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.ApiResponses;

namespace YuGiTournament.Api.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse> SendMessageToAdminAsync(string playerId, string content)
        {
            var admin = _context.Users.FirstOrDefault(u => u.Email == "admin@yugi.com");
            if (admin == null)
            {
                return new ApiResponse(false, "Admin not found.");
            }

            var player = await _context.Users.FindAsync(playerId);
            if (player == null)
            {
                return new ApiResponse(false, "Player not found.");
            }

            var message = new Message
            {
                SenderId = playerId,
                SenderFullName = $"{player.FName} {player.LName}",
                SenderPhoneNumber = player.PhoneNumber!,
                Content = content,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return new ApiResponse(true, "Message sent to admin.");
        }

        public async Task<(ApiResponse Response, List<object> Messages)> GetInboxAsync(string adminId)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderId != adminId)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.SenderFullName,
                    m.SenderPhoneNumber,
                    m.Content,
                    m.IsRead,
                    m.SentAt
                })
                .ToListAsync();

            return (new ApiResponse(true, "Messages retrieved successfully."), messages.Cast<object>().ToList());
        }

        public async Task<(ApiResponse Response, List<object> Messages)> GetReadMessagesAsync(string adminId)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderId != adminId && m.IsRead)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.SenderFullName,
                    m.SenderPhoneNumber,
                    m.Content,
                    m.IsRead,
                    m.SentAt
                })
                .ToListAsync();

            return (new ApiResponse(true, "Read messages retrieved successfully."), messages.Cast<object>().ToList());
        }

        public async Task<(ApiResponse Response, List<object> Messages)> GetUnreadMessagesAsync(string adminId)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderId != adminId && !m.IsRead)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.SenderFullName,
                    m.SenderPhoneNumber,
                    m.Content,
                    m.IsRead,
                    m.SentAt
                })
                .ToListAsync();

            return (new ApiResponse(true, "Unread messages retrieved successfully."), messages.Cast<object>().ToList());
        }

        public async Task<ApiResponse> MarkAsReadAsync(int messageId)
        {
            var message = _context.Messages.FirstOrDefault(m => m.Id == messageId);
            if (message == null)
            {
                return new ApiResponse(false, "Message not found.");
            }

            message.IsRead = true;
            await _context.SaveChangesAsync();
            return new ApiResponse(true, "Message marked as read.");
        }
    }
}