using System.Text;
using ChatBot.Chat;
using Newtonsoft.Json;

namespace ChatBot.ChatProviders.OpenAI;

public class ChatGptProvider(HttpClient httpClient, IConfiguration configuration) : IChatProvider
{
    public async Task<Message> SendMessagesAsync(IEnumerable<Message> messages)
    {
        var requestBody = new
        {
            model = "gpt-4o-mini",
            store = true,
            messages = ParseMessages(messages)
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = content
        };

        request.Headers.Add("Authorization", "Bearer " + configuration["OpenAI:ApiKey"]);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = response.Content;

        var responseJson = await responseContent.ReadAsStringAsync();
        var responseDto = JsonConvert.DeserializeObject<ChatGptResponseDto>(responseJson);

        // TODO: Throw exception if reponseDto is null or empty or if responseDto.Choices is null or empty
        return new Message(responseDto.Choices.First().Message.Content, MessageAuthor.Bot);
    }
    
    private IEnumerable<ChatGptMessageDto> ParseMessages(IEnumerable<Message> messages)
    {
        return messages.Select(message => new ChatGptMessageDto
        (
           ParseRole(message.Author),
            message.Content
        ));
    }
    
    private string ParseRole(MessageAuthor author)
    {
        switch (author)
        {
            case MessageAuthor.Bot:
                return "assistant";
            case MessageAuthor.User:
                return "user";
            default:
                throw new ArgumentOutOfRangeException(nameof(author), author, null);
        }
    }
}
