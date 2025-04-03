namespace YuGiTournament.Api.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderFullName { get; set; } = string.Empty;
        public string SenderPhoneNumber { get; set; } = string.Empty; 
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime SentAt { get; set; }
    }
}