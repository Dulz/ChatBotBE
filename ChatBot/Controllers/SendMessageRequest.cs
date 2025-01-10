namespace ChatBot.Controllers;

public record SendMessageRequest(string Message, Guid ConversationId);