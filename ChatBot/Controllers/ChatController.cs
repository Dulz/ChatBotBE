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
}