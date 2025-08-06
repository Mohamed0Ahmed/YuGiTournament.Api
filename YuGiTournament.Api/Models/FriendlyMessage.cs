namespace YuGiTournament.Api.Models
{
    public class FriendlyMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderFullName { get; set; } = string.Empty;
        public string SenderPhoneNumber { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsFromAdmin { get; set; } = false;
    }
}