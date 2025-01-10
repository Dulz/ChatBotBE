using System.Text;
using Newtonsoft.Json;

namespace ChatBot.ChatProviders.OpenAI
{
    public class ChatGptProvider(HttpClient httpClient, IConfiguration configuration) : IChatProvider
    {
        public async Task<string> SendMessageAsync(string message)
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                store = true,
                messages = new[]
                {
                    new { role = "user", content = message },
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

            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
    }
}