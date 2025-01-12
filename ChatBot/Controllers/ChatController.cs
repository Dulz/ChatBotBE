using System.Security.Claims;
using ChatBot.Chat;
using ChatBot.Controllers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatBot.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ChatController(ChatService chatService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var response =
            await chatService.SendMessageAsync(new Message(request.Message, MessageAuthor.User),
                request.ConversationId);
        return Ok(response);
    }

    [HttpGet("conversation/{id}")]
    public async Task<IActionResult> GetConversation(Guid id)
    {
        var response = await chatService.GetMessages(id);
        return Ok(response);
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