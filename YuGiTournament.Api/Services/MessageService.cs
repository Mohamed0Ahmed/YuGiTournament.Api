using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Data;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessageToAdminAsync(string playerId, string content)
        {
            var admin = _context.Users.FirstOrDefault(u => u.Email == "admin@yugi.com");
            if (admin == null)
            {
                throw new Exception("Admin not found.");
            }

            var message = new Message
            {
                SenderId = playerId,
                Content = content,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<object>> GetInboxAsync(string adminId)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderId != adminId)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.Content,
                    m.IsRead,
                    m.SentAt
                })
                .ToListAsync();

            return  messages.Cast<object>().ToList();
        }

        public async Task MarkAsReadAsync(int messageId)
        {
            var message = _context.Messages.FirstOrDefault(m => m.Id == messageId);
            if (message == null)
            {
                throw new Exception("Message not found.");
            }

            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}