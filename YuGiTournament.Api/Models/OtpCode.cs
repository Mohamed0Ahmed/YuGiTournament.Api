﻿namespace YuGiTournament.Api.Models
{
    public class OtpCode
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; 
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; } 
    }
}