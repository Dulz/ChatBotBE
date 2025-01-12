using ChatBot.Chat;
using ChatBot.ChatHistory;
using ChatBot.ChatProviders;
using Moq;

namespace ChatBotTest.Chat;

[TestFixture]
public class ChatServiceTests
{
    [SetUp]
    public void Setup()
    {
        _chatProviderMock = new Mock<IChatProvider>();
        _chatHistoryMock = new Mock<IChatHistory>();
        _chatService = new ChatService(_chatProviderMock.Object, _chatHistoryMock.Object);
    }

    private Mock<IChatProvider> _chatProviderMock;
    private Mock<IChatHistory> _chatHistoryMock;
    private ChatService _chatService;

    [Test]
    public async Task SendMessageAsync_ShouldAddMessageToHistoryAndSendToProvider()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var message = new Message("Hello", MessageAuthor.User);
        var messages = new List<Message> { message };
        var responseMessage = new Message("Response", MessageAuthor.Bot);

        _chatHistoryMock.Setup(ch => ch.UserHasConversation(userId, conversationId)).ReturnsAsync(true);
        _chatHistoryMock.Setup(ch => ch.GetMessages(conversationId)).ReturnsAsync(messages);
        _chatProviderMock.Setup(cp => cp.SendMessagesAsync(It.IsAny<IEnumerable<Message>>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _chatService.SendMessageAsync(message, userId, conversationId);

        // Assert
        Assert.That(result, Is.EqualTo(responseMessage));
        _chatHistoryMock.Verify(ch => ch.AddMessageAsync(message, conversationId), Times.Once);
        _chatHistoryMock.Verify(ch => ch.AddMessageAsync(responseMessage, conversationId), Times.Once);
        _chatProviderMock.Verify(cp => cp.SendMessagesAsync(It.Is<IEnumerable<Message>>(m => m.Contains(message))),
            Times.Once);
    }

    [Test]
    public void SendMessageAsync_ShouldThrowException_WhenUserDoesNotHaveAccessToConversation()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var message = new Message("Hello", MessageAuthor.User);

        _chatHistoryMock.Setup(ch => ch.UserHasConversation(userId, conversationId)).ReturnsAsync(false);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _chatService.SendMessageAsync(message, userId, conversationId));
    }

    [Test]
    public async Task GetMessages_ShouldReturnMessages()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var messages = new List<Message>
        {
            new("Hello", MessageAuthor.User),
            new("Hi", MessageAuthor.Bot)
        };

        _chatHistoryMock.Setup(ch => ch.UserHasConversation(userId, conversationId)).ReturnsAsync(true);
        _chatHistoryMock.Setup(ch => ch.GetMessages(conversationId)).ReturnsAsync(messages);

        // Act
        var result = await _chatService.GetMessages(userId, conversationId);

        // Assert
        Assert.That(result, Is.EqualTo(messages));
    }

    [Test]
    public void GetMessages_ShouldThrowException_WhenUserDoesNotHaveAccessToConversation()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _chatHistoryMock.Setup(ch => ch.UserHasConversation(userId, conversationId)).ReturnsAsync(false);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _chatService.GetMessages(userId, conversationId));
    }

    [Test]
    public async Task GetConversations_ShouldReturnConversations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversations = new List<Conversation>
        {
            new(Guid.NewGuid(), "Conversation 1"),
            new(Guid.NewGuid(), "Conversation 2")
        };

        _chatHistoryMock.Setup(ch => ch.GetConversations(userId)).ReturnsAsync(conversations);

        // Act
        var result = await _chatService.GetConversations(userId);

        // Assert
        Assert.That(result, Is.EqualTo(conversations));
    }

    [Test]
    public async Task CreateConversation_ShouldReturnNewConversation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationName = "New Conversation";
        var newConversation = new Conversation(Guid.NewGuid(), conversationName);

        _chatHistoryMock.Setup(ch => ch.CreateConversation(conversationName, userId)).ReturnsAsync(newConversation);

        // Act
        var result = await _chatService.CreateConversation(conversationName, userId);

        // Assert
        Assert.That(result, Is.EqualTo(newConversation));
    }
}