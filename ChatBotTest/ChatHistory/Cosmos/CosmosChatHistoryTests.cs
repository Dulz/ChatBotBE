using ChatBot.Chat;
using ChatBot.ChatHistory.Cosmos;
using ChatBot.ChatHistory.Cosmos.Dtos;
using Microsoft.Azure.Cosmos;
using Moq;

namespace ChatBotTest.ChatHistory.Cosmos;

[TestFixture]
public class CosmosChatHistoryTests
{
    [SetUp]
    public void Setup()
    {
        _databaseMock = new Mock<Database>();
        _containerMock = new Mock<Container>();
        _databaseMock.Setup(db => db.GetContainer(It.IsAny<string>())).Returns(_containerMock.Object);
        _cosmosChatHistory = new CosmosChatHistory(_databaseMock.Object);
    }

    private Mock<Database> _databaseMock;
    private Mock<Container> _containerMock;
    private CosmosChatHistory _cosmosChatHistory;

    [Test]
    public async Task AddMessageAsync_ShouldAddMessageToContainer()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var message = new Message("Hello", MessageAuthor.User);

        _containerMock.Setup(c =>
                c.CreateItemAsync(It.IsAny<MessageDto>(), It.IsAny<PartitionKey>(), null, CancellationToken.None))
            .ReturnsAsync(Mock.Of<ItemResponse<MessageDto>>());

        // Act
        await _cosmosChatHistory.AddMessageAsync(message, conversationId);

        // Assert
        _containerMock.Verify(c => c.CreateItemAsync(It.Is<MessageDto>(m =>
                m.Content == message.Content &&
                m.Author == message.Author.ToString() &&
                m.ConversationId == conversationId),
            It.Is<PartitionKey>(pk => pk.Equals(new PartitionKey(conversationId.ToString()))),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetMessages_ShouldReturnEmptyList_WhenNoMessagesExist()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var emptyFeedResponseMock = CreateFeedResponseMock(new List<MessageDto>());
        var emptyIteratorMock = CreateFeedIteratorMock(emptyFeedResponseMock);

        _containerMock.Setup(c => c.GetItemQueryIterator<MessageDto>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(emptyIteratorMock.Object);

        // Act
        var result = await _cosmosChatHistory.GetMessages(conversationId);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetMessages_ShouldReturnFilteredMessages_ByConversationId()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var messages = new List<MessageDto>
        {
            new(Guid.NewGuid(), conversationId, MessageAuthor.User, "Hello"),
            new(Guid.NewGuid(), conversationId, MessageAuthor.Bot, "How can I help you?")
        };
        var feedResponseMock = CreateFeedResponseMock(messages);
        var iteratorMock = CreateFeedIteratorMock(feedResponseMock);

        _containerMock.Setup(c => c.GetItemQueryIterator<MessageDto>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(iteratorMock.Object);

        // Act
        var result = (await _cosmosChatHistory.GetMessages(conversationId)).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(m => m.Content == "Hello"));
            Assert.That(result.Any(m => m.Content == "How can I help you?"));
        });
    }

    [Test]
    public async Task GetConversations_ShouldReturnEmptyList_WhenNoConversationsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emptyFeedResponseMock = CreateFeedResponseMock(new List<ConversationDto>());
        var emptyIteratorMock = CreateFeedIteratorMock(emptyFeedResponseMock);

        _containerMock.Setup(c => c.GetItemQueryIterator<ConversationDto>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(emptyIteratorMock.Object);

        // Act
        var result = await _cosmosChatHistory.GetConversations(userId);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetConversations_ShouldReturnFilteredConversations_ByUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversations = new List<ConversationDto>
        {
            new(Guid.NewGuid(), "Conversation 1", userId),
            new(Guid.NewGuid(), "Conversation 2", userId)
        };
        var feedResponseMock = CreateFeedResponseMock(conversations);
        var iteratorMock = CreateFeedIteratorMock(feedResponseMock);

        _containerMock.Setup(c => c.GetItemQueryIterator<ConversationDto>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(iteratorMock.Object);

        // Act
        var result = (await _cosmosChatHistory.GetConversations(userId)).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(c => c.Name == "Conversation 1"));
            Assert.That(result.Any(c => c.Name == "Conversation 2"));
        });
    }

    [Test]
    public async Task CreateConversation_ShouldReturnNewConversation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationName = "New Conversation";
        var conversationDto = new ConversationDto(Guid.NewGuid(), conversationName, userId);
        var itemResponseMock = new Mock<ItemResponse<ConversationDto>>();
        itemResponseMock.Setup(ir => ir.Resource).Returns(conversationDto);

        _containerMock.Setup(c => c.CreateItemAsync(It.IsAny<ConversationDto>(), It.IsAny<PartitionKey>(), null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemResponseMock.Object);

        // Act
        var result = await _cosmosChatHistory.CreateConversation(conversationName, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo(conversationName));
            Assert.That(result.Id, Is.EqualTo(conversationDto.Id));
        });
        _containerMock.Verify(c => c.CreateItemAsync(It.Is<ConversationDto>(dto =>
                dto.Name == conversationName &&
                dto.UserId == userId),
            It.Is<PartitionKey>(pk => pk.Equals(new PartitionKey(userId.ToString()))),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UserHasConversation_ShouldReturnTrue_WhenConversationExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversationDto = new ConversationDto(conversationId, "Test Conversation", userId);

        var feedResponseMock = CreateFeedResponseMock(new List<ConversationDto> { conversationDto });
        var feedIteratorMock = CreateFeedIteratorMock(feedResponseMock);

        _containerMock.Setup(c => c.GetItemQueryIterator<ConversationDto>(It.IsAny<QueryDefinition>(),
                It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(feedIteratorMock.Object);

        // Act
        var result = await _cosmosChatHistory.UserHasConversation(userId, conversationId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UserHasConversation_ShouldReturnFalse_WhenConversationDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var feedResponseMock = CreateFeedResponseMock(new List<ConversationDto>());
        var feedIteratorMock = CreateFeedIteratorMock(feedResponseMock);

        _containerMock.Setup(c => c.GetItemQueryIterator<ConversationDto>(It.IsAny<QueryDefinition>(),
                It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(feedIteratorMock.Object);
        
        // Act
        var result = await _cosmosChatHistory.UserHasConversation(userId, conversationId);

        // Assert
        Assert.That(result, Is.False);
    }

    private Mock<FeedResponse<T>> CreateFeedResponseMock<T>(IList<T> items)
    {
        var feedResponseMock = new Mock<FeedResponse<T>>();
        using var enumerator = items.GetEnumerator();
        feedResponseMock.Setup(fr => fr.GetEnumerator()).Returns(enumerator);
        feedResponseMock.Setup(fr => fr.Count).Returns(items.Count);
        return feedResponseMock;
    }

    private Mock<FeedIterator<T>> CreateFeedIteratorMock<T>(Mock<FeedResponse<T>> feedResponseMock)
    {
        var feedIteratorMock = new Mock<FeedIterator<T>>();
        feedIteratorMock.SetupSequence(fi => fi.HasMoreResults).Returns(true).Returns(false);
        feedIteratorMock.Setup(fi => fi.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedResponseMock.Object);
        return feedIteratorMock;
    }
}