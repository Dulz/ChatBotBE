using System.Security.Claims;
using ChatBot.Chat;
using ChatBot.Controllers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatBot.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ChatController(IChatService chatService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var response =
                await chatService.SendMessageAsync(new Message(request.Message, MessageAuthor.User),
                    userId.Value,
                    request.ConversationId);

            return Ok(response);
        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(e.Message);
        }
    }

    [HttpGet("conversation/{id}")]
    public async Task<IActionResult> GetConversation(Guid id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var response = await chatService.GetMessages(userId.Value, id);
            return Ok(response);
        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(e.Message);
        }
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var response = await chatService.GetConversations(userId.Value);

        return Ok(response);
    }

    [HttpPost("conversation")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var response = await chatService.CreateConversation(request.Name, userId.Value);

        return Ok(response);
    }

    private Guid? GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;
        return Guid.Parse(userId);
    }
}