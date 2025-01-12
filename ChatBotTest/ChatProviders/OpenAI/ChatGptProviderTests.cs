using System.Net;
using ChatBot.Chat;
using ChatBot.ChatProviders.OpenAI;
using ChatBot.ChatProviders.OpenAI.Dtos;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace ChatBotTest.ChatProviders.OpenAI;

[TestFixture]
public class ChatGptProviderTests
{
    [SetUp]
    public void Setup()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.SetupGet(c => c["OpenAI:EndpointUri"]).Returns("https://api.openai.com/v1/chat/completions");
        _configurationMock.SetupGet(c => c["OpenAI:ApiKey"]).Returns("test-api-key");

        _chatGptProvider = new ChatGptProvider(_httpClient, _configurationMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private Mock<IConfiguration> _configurationMock;
    private ChatGptProvider _chatGptProvider;

    [Test]
    public async Task SendMessagesAsync_ShouldReturnBotMessage()
    {
        // Arrange
        var messages = new List<Message>
        {
            new("Hello", MessageAuthor.User)
        };

        var responseDto = new ChatGptResponseDto
        ("id",
            "object",
            "model",
            "created",
            new List<ChatGptChoiceDto>
            {
                new(
                    new ChatGptMessageDto("user", "Hi there!"),
                    "stop"
                )
            }
        );

        var responseContent = new StringContent(JsonConvert.SerializeObject(responseDto));
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _chatGptProvider.SendMessagesAsync(messages);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Content, Is.EqualTo("Hi there!"));
            Assert.That(result.Author, Is.EqualTo(MessageAuthor.Bot));
        });
    }

    [Test]
    public void SendMessagesAsync_ShouldThrowException_WhenNoChoicesReturned()
    {
        // Arrange
        var messages = new List<Message>
        {
            new("Hello", MessageAuthor.User)
        };

        var responseContent = new StringContent(JsonConvert.SerializeObject(null));
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _chatGptProvider.SendMessagesAsync(messages));
        Assert.That(ex.Message, Is.EqualTo("No choices returned from OpenAI"));
    }
}