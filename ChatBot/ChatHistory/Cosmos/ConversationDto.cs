using Newtonsoft.Json;

namespace ChatBot.ChatHistory.Cosmos;

public record ConversationDto(
    [property: JsonProperty("id")] Guid Id,
    [property: JsonProperty("name")] string Name,
    [property: JsonProperty("userId")] Guid UserId
);
