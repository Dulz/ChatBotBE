using ChatBot.ChatService;
using Microsoft.Azure.Cosmos;

namespace ChatBot.ChatHistory.Cosmos;

public class CosmosChatHistory(Database database) : IChatHistory
{
    // The name of the database and container we will create

    private const string DatabaseId = "ChatDb";
    private const string ContainerId = "Chat";


    public async Task AddMessageAsync(Message message, Guid conversationId)
    {
        var chatContainer = await GetChatContainer();
        var messageDto = new MessageDto(Guid.NewGuid().ToString(), conversationId, message.Author, message.Content);
        await chatContainer.CreateItemAsync(messageDto, new PartitionKey(messageDto.ConversationId.ToString()));
    }

    public async Task<IEnumerable<Message>> GetMessages(Guid conversationId)
    {
        var chatContainer = await GetChatContainer();
        var query = new QueryDefinition("SELECT * FROM c WHERE c.conversationId = @conversationId")
            .WithParameter("@conversationId", conversationId.ToString());

        var messages = new List<Message>();
        var iterator = chatContainer.GetItemQueryIterator<MessageDto>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            messages.AddRange(response.ToList().Select(dto => new Message(dto.Content, Enum.Parse<MessageAuthor>(dto.Author))));
        }

        return messages;
    }

    private async Task<Container> GetChatContainer()
    {
        await database.CreateContainerIfNotExistsAsync(ContainerId, "/conversationId");
        return database.GetContainer(ContainerId);
    }
}