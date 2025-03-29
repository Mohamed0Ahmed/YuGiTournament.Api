using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.Identities;

namespace YuGiTournament.Api.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MessageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse> SendMessageToAdminAsync(string playerId, string content)
        {
            var admin = _unitOfWork.GetRepository<ApplicationUser>().GetAll().FirstOrDefault(u => u.Email == "admin@yugi.com");
            if (admin == null)
            {
                return new ApiResponse(false, "Admin not found.");
            }

            var player = await _unitOfWork.GetRepository<ApplicationUser>().GetByIdAsync(playerId);
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

            await _unitOfWork.GetRepository<Message>().AddAsync(message);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse(true, "Message sent to admin.");
        }

        public async Task<(ApiResponse Response, List<object> Messages)> GetInboxAsync(string adminId)
        {
            var messages = await _unitOfWork.GetRepository<Message>()
                .GetAll()
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
            var messages = await _unitOfWork.GetRepository<Message>()
                .GetAll()
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
            var messages = await _unitOfWork.GetRepository<Message>()
                .GetAll()
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

        public async Task<ApiResponse> MarkAsync(int messageId, bool marked)
        {
            var message = _unitOfWork.GetRepository<Message>().GetAll().FirstOrDefault(m => m.Id == messageId);
            if (message == null)
                return new ApiResponse(false, "Message not found.");

            if (marked)
            {
                if (message.IsRead == true)
                    return new ApiResponse(false, "This message is already marked as read.");

                message.IsRead = true;
                await _unitOfWork.SaveChangesAsync();
                return new ApiResponse(true, "Message marked as read.");
            }
            else
            {
                if (message.IsRead == false)
                    return new ApiResponse(false, "This message is already marked as unread.");

                message.IsRead = false;
                await _unitOfWork.SaveChangesAsync();
                return new ApiResponse(true, "Message marked as unread.");
            }
        }


    }
}

