using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.Services.Abstractions;
using System.Security.Claims;
using YuGiTournament.Api.ApiResponses;
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
                return Unauthorized(new ApiResponse(false, "Unauthorized."));
            }

            var response = await _messageService.SendMessageToAdminAsync(playerId, request.Content);
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
      

            var (response, messages) = await _messageService.GetInboxAsync();
            return Ok(new { response.Success, response.Message, Messages = messages });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("read-messages")]
        public async Task<IActionResult> GetReadMessages()
        {
           

            var (response, messages) = await _messageService.GetReadMessagesAsync();
            return Ok(new { response.Success, response.Message, Messages = messages });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("unread-messages")]
        public async Task<IActionResult> GetUnreadMessages()
        {

            var (response, messages) = await _messageService.GetUnreadMessagesAsync();
            return Ok(new { response.Success, response.Message, Messages = messages });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("mark/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId, [FromBody] MarkMessageDto request)
        {
            var response = await _messageService.MarkAsync(messageId, request.Marked);
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
            var response = await _messageService.SoftDelete(messageId, request.Marked);
            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }


    }


}