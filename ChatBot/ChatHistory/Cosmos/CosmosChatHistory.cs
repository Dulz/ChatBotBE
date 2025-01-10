using ChatBot.ChatService;
using Microsoft.Azure.Cosmos;

namespace ChatBot.ChatHistory.Cosmos;

public class CosmosChatHistory(Database database) : IChatHistory
{
    // The name of the database and container we will create

    private const string DatabaseId = "ChatDb";
    private const string ContainerId = "Chat";


    public async Task AddMessageAsync(Message message, MessageAuthor messageAuthor, Guid conversationId)
    {
        var chatContainer = await GetChatContainer();
        var messageDto = new MessageDto(Guid.NewGuid().ToString(), conversationId, messageAuthor, message.Content);
        await chatContainer.CreateItemAsync(messageDto, new PartitionKey(messageDto.ConversationId.ToString()));
    }

    public IEnumerable<string> GetMessages(Guid conversationId)
    {
        throw new NotImplementedException();
    }

    private async Task<Container> GetChatContainer()
    {
        await database.CreateContainerIfNotExistsAsync(ContainerId, "/conversationId");
        return database.GetContainer(ContainerId);
    }
}