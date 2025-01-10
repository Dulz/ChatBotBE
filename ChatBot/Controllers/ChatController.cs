using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ChatBot.ChatProviders.OpenAI;

namespace ChatBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController(ChatGptProvider chatGptProvider) : ControllerBase
    {
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage()
        {
            var response = await chatGptProvider.SendMessageAsync("");
            return Ok(response);
        }
    }
}