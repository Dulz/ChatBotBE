using System.Text;
using ChatBot.Chat;
using ChatBot.ChatProviders.OpenAI.Dtos;
using Newtonsoft.Json;

namespace ChatBot.ChatProviders.OpenAI;

public class ChatGptProvider(HttpClient httpClient, IConfiguration configuration) : IChatProvider
{
    private const string Model = "gpt-4o-mini";

    public async Task<Message> SendMessagesAsync(IEnumerable<Message> messages)
    {
        var request = BuildRequest(messages);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseDto = JsonConvert.DeserializeObject<ChatGptResponseDto>(responseJson);
        
        if (responseDto is null || !responseDto.Choices.Any())
        {
            throw new Exception("No choices returned from OpenAI");
        }
        
        return new Message(responseDto.Choices.First().Message.Content, MessageAuthor.Bot);
    }

    private HttpRequestMessage BuildRequest(IEnumerable<Message> messages)
    {
        var requestBody = new
        {
            model = Model,
            store = true,
            messages = ParseMessages(messages)
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, configuration["OpenAI:EndpointUri"])
        {
            Content = content
        };

        request.Headers.Add("Authorization", "Bearer " + configuration["OpenAI:ApiKey"]);
        return request;
    }

    private static IEnumerable<ChatGptMessageDto> ParseMessages(IEnumerable<Message> messages)
    {
        return messages.Select(message => new ChatGptMessageDto
        (
            ParseRole(message.Author),
            message.Content
        ));
    }

    private static string ParseRole(MessageAuthor author)
    {
        return author switch
        {
            MessageAuthor.Bot => "assistant",
            MessageAuthor.User => "user",
            _ => throw new ArgumentOutOfRangeException(nameof(author), author, null)
        };
    }
}