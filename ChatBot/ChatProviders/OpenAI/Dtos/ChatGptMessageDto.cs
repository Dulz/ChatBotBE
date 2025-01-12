using Newtonsoft.Json;

namespace ChatBot.ChatProviders.OpenAI.Dtos;

internal record ChatGptMessageDto(
    [property: JsonProperty("role")] string Role,
    [property: JsonProperty("content")] string Content);