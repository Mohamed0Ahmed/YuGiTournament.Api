using Microsoft.AspNetCore.Mvc;

namespace YuGiTournament.Api.Controllers
{
    public class PlayerController : BaseApiController
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
