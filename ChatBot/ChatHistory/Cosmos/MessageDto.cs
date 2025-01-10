using ChatBot.Chat;
using Newtonsoft.Json;

namespace ChatBot.ChatHistory.Cosmos;

internal record MessageDto(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("conversationId")]
    Guid ConversationId,
    string Author,
    string Content
)
{
    [JsonConstructor]
    public MessageDto(string id, Guid conversationId, MessageAuthor author, string content)
        : this(id, conversationId, author.ToString(), content)
    {
    }
}