using System.Security.Claims;
using ChatBot.Chat;
using ChatBot.Controllers;
using ChatBot.Controllers.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatBotTest.Controllers;

[TestFixture]
public class ChatControllerTests
{
    [SetUp]
    public void Setup()
    {
        _chatServiceMock = new Mock<IChatService>();
        _chatController = new ChatController(_chatServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                    ]))
                }
            }
        };
    }

    private Mock<IChatService> _chatServiceMock;
    private ChatController _chatController;

    [Test]
    public async Task SendMessage_ShouldReturnOk_WhenMessageIsSent()
    {
        // Arrange
        var request = new SendMessageRequest("Hello", Guid.NewGuid());
        var responseMessage = new Message("Response", MessageAuthor.Bot);

        _chatServiceMock.Setup(cs => cs.SendMessageAsync(It.IsAny<Message>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _chatController.SendMessage(request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(responseMessage, Is.EqualTo(okResult.Value));
    }

    [Test]
    public async Task SendMessage_ShouldReturnUnauthorized_WhenUserIsNotAuthorized()
    {
        // Arrange
        _chatController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _chatController.SendMessage(new SendMessageRequest("Hello", Guid.NewGuid()));

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public async Task SendMessage_ShouldReturnUnauthorized_WhenChatServiceThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new SendMessageRequest("Hello", Guid.NewGuid());
        var userId = Guid.NewGuid();
        _chatController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ]));

        _chatServiceMock.Setup(cs => cs.SendMessageAsync(It.IsAny<Message>(), userId, request.ConversationId))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have access to this conversation"));

        // Act
        var result = await _chatController.SendMessage(request);

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null);
        Assert.That(unauthorizedResult.Value, Is.EqualTo("User does not have access to this conversation"));
    }

    [Test]
    public async Task GetConversation_ShouldReturnOk_WhenConversationIsRetrieved()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var messages = new List<Message>
        {
            new("Hello", MessageAuthor.User),
            new("Hi", MessageAuthor.Bot)
        };

        _chatServiceMock.Setup(cs => cs.GetMessages(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(messages);

        // Act
        var result = await _chatController.GetConversation(conversationId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(messages, Is.EqualTo(okResult.Value));
    }

    [Test]
    public async Task GetConversation_ShouldReturnUnauthorized_WhenUserIsNotAuthorized()
    {
        // Arrange
        _chatController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _chatController.GetConversation(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public async Task GetConversation_ShouldReturnUnauthorized_WhenChatServiceThrowsUnauthorizedAccessException()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _chatController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ]));

        _chatServiceMock.Setup(cs => cs.GetMessages(userId, conversationId))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have access to this conversation"));

        // Act
        var result = await _chatController.GetConversation(conversationId);

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null);
        Assert.That(unauthorizedResult.Value, Is.EqualTo("User does not have access to this conversation"));
    }

    [Test]
    public async Task GetConversations_ShouldReturnOk_WhenConversationsAreRetrieved()
    {
        // Arrange
        var conversations = new List<Conversation>
        {
            new(Guid.NewGuid(), "Conversation 1"),
            new(Guid.NewGuid(), "Conversation 2")
        };

        _chatServiceMock.Setup(cs => cs.GetConversations(It.IsAny<Guid>())).ReturnsAsync(conversations);

        // Act
        var result = await _chatController.GetConversations();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(conversations, Is.EqualTo(okResult.Value));
    }

    [Test]
    public async Task GetConversations_ShouldReturnUnauthorized_WhenUserIsNotAuthorized()
    {
        // Arrange
        _chatController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _chatController.GetConversations();

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public async Task CreateConversation_ShouldReturnOk_WhenConversationIsCreated()
    {
        // Arrange
        var request = new CreateConversationRequest("New Conversation");
        var newConversation = new Conversation(Guid.NewGuid(), request.Name);

        _chatServiceMock.Setup(cs => cs.CreateConversation(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(newConversation);

        // Act
        var result = await _chatController.CreateConversation(request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(newConversation, Is.EqualTo(okResult.Value));
    }

    [Test]
    public async Task CreateConversation_ShouldReturnUnauthorized_WhenUserIsNotAuthorized()
    {
        // Arrange
        _chatController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _chatController.CreateConversation(new CreateConversationRequest("New Conversation"));

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
    }
}