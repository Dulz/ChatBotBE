using ChatBot.Chat;
using Newtonsoft.Json;

namespace ChatBot.ChatHistory.Cosmos.Dtos;

internal record MessageDto(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("conversationId")]
    Guid ConversationId,
    string Author,
    string Content
)
{
    [JsonConstructor]
    public MessageDto(Guid id, Guid conversationId, MessageAuthor author, string content)
        : this(id.ToString(), conversationId, author.ToString(), content)
    {
    }
}