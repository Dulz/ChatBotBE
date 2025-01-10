using ChatBot.ChatProviders.OpenAI;
using Microsoft.AspNetCore.Mvc;

namespace ChatBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController(ChatGptProvider chatGptProvider) : ControllerBase
    {
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage()
        {
            // var response = await chatGptProvider.SendMessageAsync("");
            return Ok();
        }
    }
}