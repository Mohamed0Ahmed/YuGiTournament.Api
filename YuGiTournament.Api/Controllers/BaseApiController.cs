using Microsoft.AspNetCore.Mvc;

namespace YuGiTournament.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { Message = "API is working!", Timestamp = DateTime.UtcNow });
        }
    }
}
