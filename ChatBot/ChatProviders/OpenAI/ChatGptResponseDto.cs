namespace ChatBot.ChatProviders.OpenAI;

internal record ChatGptResponseDto(
    string Id,
    string Object,
    string Model,
    string Created,
    IEnumerable<ChatGptChoiceDto> Choices
);