using Microsoft.AspNetCore.Mvc;

namespace YuGiTournament.Api.Controllers
{
    public class MatchController : BaseApiController
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
