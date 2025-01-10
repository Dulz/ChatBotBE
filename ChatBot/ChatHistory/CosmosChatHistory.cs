using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace ChatBot.ChatHistory;

public class CosmosChatHistory(Database database) : IChatHistory
{
    // The name of the database and container we will create
    
    private const string DatabaseId = "ChatDb";
    private const string ContainerId = "Chat";


    public async Task AddMessageAsync(Message message)
    {
        var chatContainer = await GetChatContainer();
       await chatContainer.CreateItemAsync(message, new PartitionKey(message.ConversationId.ToString()));
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

public enum MessageAuthor
{
    User,
    Bot
}

public class Message(string id, Guid conversationId, MessageAuthor author, string content)
{
    
    [JsonProperty("id")]
    public string Id { get; } = id;
    
    [JsonProperty("conversationId")]
    public Guid ConversationId { get; } = conversationId;

    public MessageAuthor Author { get; } = author;
    public string Content { get; } = content;
}