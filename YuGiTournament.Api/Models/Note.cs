namespace YuGiTournament.Api.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public bool IsHidden { get; set; }
    }
}
