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

        public async Task<(ApiResponse Response, List<Message> Messages)> GetInboxAsync()
        {
            var messages = await _unitOfWork.GetRepository<Message>()
                .GetAll()
                .Where(m => !m.IsDeleted).ToListAsync();

            return (new ApiResponse(true, "Messages retrieved successfully."), messages);
        }

        public async Task<(ApiResponse Response, List<Message> Messages)> GetReadMessagesAsync()
        {
            var messages = await _unitOfWork.GetRepository<Message>()
                .GetAll()
                .Where(m => m.IsRead && !m.IsDeleted)
                .ToListAsync();

            return (new ApiResponse(true, "Read messages retrieved successfully."), messages);
        }

        public async Task<(ApiResponse Response, List<Message> Messages)> GetUnreadMessagesAsync()
        {
            var messages = await _unitOfWork.GetRepository<Message>()
                .GetAll()
                .Where(m => !m.IsRead && !m.IsDeleted)
                .ToListAsync();

            return (new ApiResponse(true, "Unread messages retrieved successfully."), messages);
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

        public async Task<ApiResponse> SoftDelete(int messageId, bool marked)
        {

            var message = _unitOfWork.GetRepository<Message>().GetAll().FirstOrDefault(m => m.Id == messageId);
            if (message == null)
                return new ApiResponse(false, "Message not found.");
            if (marked)
            {
                if (message.IsDeleted == true)
                    return new ApiResponse(false, "This message is already deleted.");

                message.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();
                return new ApiResponse(true, "Message deleted.");
            }
            else
            {
                if (message.IsDeleted == false)
                    return new ApiResponse(false, "This message is already not deleted.");

                message.IsDeleted = false;
                await _unitOfWork.SaveChangesAsync();
                return new ApiResponse(true, "Message returned.");
            }
        }


    }
}

