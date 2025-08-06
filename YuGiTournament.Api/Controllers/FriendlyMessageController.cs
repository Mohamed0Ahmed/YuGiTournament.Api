using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.Abstractions;
using System.Security.Claims;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendlyMessageController : ControllerBase
    {
        private readonly IFriendlyMessageService _friendlyMessageService;

        public FriendlyMessageController(IFriendlyMessageService friendlyMessageService)
        {
            _friendlyMessageService = friendlyMessageService;
        }

        [Authorize(Roles = "Player")]
        [HttpPost("send")]
        public async Task<IActionResult> SendMessageToAdmin([FromBody] SendMessageRequestDto request)
        {
            var playerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(new ApiResponse(false, "Unauthorized."));
            }

            var response = await _friendlyMessageService.SendMessageToAdminAsync(playerId, request.Content);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox()
        {
            var (response, messages) = await _friendlyMessageService.GetInboxAsync();
            return Ok(new { response.Success, response.Message, Messages = messages });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("read-messages")]
        public async Task<IActionResult> GetReadMessages()
        {
            var (response, messages) = await _friendlyMessageService.GetReadMessagesAsync();
            return Ok(new { response.Success, response.Message, Messages = messages });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("unread-messages")]
        public async Task<IActionResult> GetUnreadMessages()
        {
            var (response, messages) = await _friendlyMessageService.GetUnreadMessagesAsync();
            return Ok(new { response.Success, response.Message, Messages = messages });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("mark/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId, [FromBody] MarkMessageDto request)
        {
            var response = await _friendlyMessageService.MarkAsync(messageId, request.Marked);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId, [FromBody] MarkMessageDto request)
        {
            var response = await _friendlyMessageService.SoftDelete(messageId, request.Marked);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Player")]
        [HttpGet("my-messages")]
        public async Task<IActionResult> GetPlayerMessages()
        {
            var playerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(new ApiResponse(false, "Unauthorized."));
            }

            var (response, messages) = await _friendlyMessageService.GetPlayerMessagesAsync(playerId);
            return Ok(new { response.Success, response.Message, Messages = messages });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("reply/{playerId}")]
        public async Task<IActionResult> SendAdminReply(string playerId, [FromBody] SendMessageRequestDto request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(new ApiResponse(false, "Unauthorized."));
            }

            var response = await _friendlyMessageService.SendAdminReplyAsync(adminId, playerId, request.Content);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
    }
}