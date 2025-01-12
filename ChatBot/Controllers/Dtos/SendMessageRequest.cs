namespace ChatBot.Controllers.Dtos;

public record SendMessageRequest(string Message, Guid ConversationId);