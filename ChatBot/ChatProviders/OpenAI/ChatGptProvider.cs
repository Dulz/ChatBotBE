using System.Text;
using ChatBot.ChatService;
using Newtonsoft.Json;

namespace ChatBot.ChatProviders.OpenAI
{
    public class ChatGptProvider(HttpClient httpClient, IConfiguration configuration) : IChatProvider
    {
        public async Task<Message> SendMessageAsync(Message message)
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                store = true,
                messages = new[]
                {
                    new { role = "user", content = message.Content },
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = content
            };

            request.Headers.Add("Authorization", "Bearer "+ configuration["OpenAI:ApiKey"]);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent =  response.Content;
            
            var responseJson = await responseContent.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<ChatGptResponseDto>(responseJson);

            // TODO: Throw exception if reponseDto is null or empty or if responseDto.Choices is null or empty
            return new Message(responseDto.Choices.First().Message.Content, MessageAuthor.Bot);

        }
    }
}