using Newtonsoft.Json;

namespace ChatBot.ChatProviders.OpenAI;

internal record ChatGptMessageDto(
    [property: JsonProperty("role")] string Role,
    [property: JsonProperty("content")] string Content);