using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.Services.Abstractions;
using System.Security.Claims;
using YuGiTournament.Api.DTOs;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [Authorize(Roles = "Player")]
        [HttpPost("send")]
        public async Task<IActionResult> SendMessageToAdmin([FromBody] SendMessageRequestDto request)
        {
            var playerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized();
            }

            try
            {
                await _messageService.SendMessageToAdminAsync(playerId, request.Content);
                return Ok(new { message = "Message sent to admin." });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox()
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized();
            }

            var messages = await _messageService.GetInboxAsync(adminId);
            return Ok(messages);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("mark/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            try
            {
                await _messageService.MarkAsReadAsync(messageId);
                return Ok(new { message = "Message marked as read." });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }

    
}