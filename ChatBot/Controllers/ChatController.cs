using ChatBot.Chat;
using Microsoft.AspNetCore.Mvc;

namespace ChatBot.Controllers;

[ApiController]
[Route("[controller]")]
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
        var response = await chatService.GetConversations(Guid.Parse("00000000-0000-0000-0000-000000000000"));
        return Ok(response);
    }
    
    
    [HttpPost("conversation")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var response = await chatService.CreateConversation(request.Name, Guid.Parse("00000000-0000-0000-0000-000000000000"));
        return Ok(response);
    }
    
}