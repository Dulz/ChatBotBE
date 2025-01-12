using ChatBot.Chat;
using ChatBot.ChatHistory.Cosmos.Dtos;
using Microsoft.Azure.Cosmos;

namespace ChatBot.ChatHistory.Cosmos;

public class CosmosChatHistory(Database database) : IChatHistory
{
    public const string ContainerId = "Chat";
    public const string UserContainerId = "User";


    public async Task AddMessageAsync(Message message, Guid conversationId)
    {
        var chatContainer = database.GetContainer(ContainerId);
        var messageDto = new MessageDto(Guid.NewGuid(), conversationId, message.Author, message.Content);
        await chatContainer.CreateItemAsync(messageDto, new PartitionKey(messageDto.ConversationId.ToString()));
    }

    public async Task<IEnumerable<Message>> GetMessages(Guid conversationId)
    {
        var chatContainer = database.GetContainer(ContainerId);
        var query = new QueryDefinition("SELECT * FROM c WHERE c.conversationId = @conversationId")
            .WithParameter("@conversationId", conversationId.ToString());

        var messages = new List<Message>();
        var iterator = chatContainer.GetItemQueryIterator<MessageDto>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            messages.AddRange(response.ToList()
                .Select(dto => new Message(dto.Content, Enum.Parse<MessageAuthor>(dto.Author))));
        }

        return messages;
    }

    public async Task<IEnumerable<Conversation>> GetConversations(Guid userId)
    {
        var chatContainer = database.GetContainer(UserContainerId);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
            .WithParameter("@userId", userId.ToString());

        var conversations = new List<Conversation>();
        var iterator = chatContainer.GetItemQueryIterator<ConversationDto>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            conversations.AddRange(response.ToList()
                .Select(dto => new Conversation(dto.Id, dto.Name)));
        }

        return conversations;
    }

    public async Task<Conversation> CreateConversation(string name, Guid userId)
    {
        var chatContainer = database.GetContainer(UserContainerId);
        var conversationDto = new ConversationDto(Guid.NewGuid(), name, userId);
        var item = await chatContainer.CreateItemAsync(conversationDto, new PartitionKey(userId.ToString()));
        return new Conversation(item.Resource.Id, item.Resource.Name);
    }
}