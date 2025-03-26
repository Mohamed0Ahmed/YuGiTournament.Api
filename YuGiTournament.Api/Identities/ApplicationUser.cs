using Microsoft.AspNetCore.Identity;

namespace YuGiTournament.Api.Identities
{
    public class ApplicationUser : IdentityUser
    {


        public string FName { get; set; } = null!;
        public string LName { get; set; } = null!;
        public bool IsAgree { get; set; }



    }
}
