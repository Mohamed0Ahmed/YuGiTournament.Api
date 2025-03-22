namespace YuGiTournament.Api.Models
{
    public class Player
    {

        public int PlayerId { get; set; }  
        public string FullName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Points { get; set; }
    }
}
