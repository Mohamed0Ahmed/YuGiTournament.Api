using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Identities;

namespace YuGiTournament.Api.Services
{
    public class FriendlyMessageService : IFriendlyMessageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FriendlyMessageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse> SendMessageToAdminAsync(string playerId, string content)
        {
          

            var player = await _unitOfWork.GetRepository<ApplicationUser>().GetByIdAsync(playerId);
            if (player == null)
            {
                return new ApiResponse(false, "Player not found.");
            }

            var message = new FriendlyMessage
            {
                SenderId = playerId,
                SenderFullName = $"{player.FName} {player.LName}",
                SenderPhoneNumber = player.PhoneNumber!,
                Content = content,
                IsRead = false,
                IsFromAdmin = false,
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<FriendlyMessage>().AddAsync(message);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse(true, "Message sent to admin.");
        }

        public async Task<(ApiResponse Response, List<FriendlyMessage> Messages)> GetInboxAsync()
        {
            var messages = await _unitOfWork.GetRepository<FriendlyMessage>()
                .GetAll()
                .Where(m => !m.IsDeleted)
                .ToListAsync();

            return (new ApiResponse(true, "Messages retrieved successfully."), messages);
        }

        public async Task<(ApiResponse Response, List<FriendlyMessage> Messages)> GetReadMessagesAsync()
        {
            var messages = await _unitOfWork.GetRepository<FriendlyMessage>()
                .GetAll()
                .Where(m => !m.IsDeleted && m.IsRead)
                .ToListAsync();

            return (new ApiResponse(true, "Read messages retrieved successfully."), messages);
        }

        public async Task<(ApiResponse Response, List<FriendlyMessage> Messages)> GetUnreadMessagesAsync()
        {
            var messages = await _unitOfWork.GetRepository<FriendlyMessage>()
                .GetAll()
                .Where(m => !m.IsDeleted && !m.IsRead)
                .ToListAsync();

            return (new ApiResponse(true, "Unread messages retrieved successfully."), messages);
        }

        public async Task<ApiResponse> MarkAsync(int messageId, bool marked)
        {
            var message = _unitOfWork.GetRepository<FriendlyMessage>().GetAll().FirstOrDefault(m => m.Id == messageId);
            if (message == null)
            {
                return new ApiResponse(false, "Message not found.");
            }

            message.IsRead = marked;
            _unitOfWork.GetRepository<FriendlyMessage>().Update(message);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse(true, "Message marked successfully.");
        }

        public async Task<ApiResponse> SoftDelete(int messageId, bool marked)
        {
            var message = _unitOfWork.GetRepository<FriendlyMessage>().GetAll().FirstOrDefault(m => m.Id == messageId);
            if (message == null)
            {
                return new ApiResponse(false, "Message not found.");
            }

            message.IsDeleted = marked;
            _unitOfWork.GetRepository<FriendlyMessage>().Update(message);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse(true, "Message deleted successfully.");
        }

        public async Task<(ApiResponse Response, List<FriendlyMessage> Messages)> GetPlayerMessagesAsync(string playerId)
        {
            var messages = await _unitOfWork.GetRepository<FriendlyMessage>()
                .GetAll()
                .Where(m => !m.IsDeleted && m.SenderId == playerId)
                .ToListAsync();

            return (new ApiResponse(true, "Player messages retrieved successfully."), messages);
        }

        public async Task<ApiResponse> SendAdminReplyAsync(string adminId, string playerId, string content)
        {
            var admin = await _unitOfWork.GetRepository<ApplicationUser>().GetByIdAsync(adminId);
            if (admin == null)
            {
                return new ApiResponse(false, "Admin not found.");
            }

            var player = await _unitOfWork.GetRepository<ApplicationUser>().GetByIdAsync(playerId);
            if (player == null)
            {
                return new ApiResponse(false, "Player not found.");
            }

            var message = new FriendlyMessage
            {
                SenderId = playerId,
                SenderFullName = "Admin",
                SenderPhoneNumber = admin.PhoneNumber ?? "N/A",
                Content = content,
                IsRead = true,
                IsFromAdmin = true,
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<FriendlyMessage>().AddAsync(message);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse(true, "Admin reply sent.");
        }
    }
}