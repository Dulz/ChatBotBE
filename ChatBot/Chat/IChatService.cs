namespace ChatBot.Chat;

public interface IChatService
{
    Task<Message> SendMessageAsync(Message message, Guid userId, Guid conversationId);
    
    Task<IEnumerable<Message>> GetMessages(Guid userId, Guid conversationId);
    
    Task<IEnumerable<Conversation>> GetConversations(Guid userId);
    
    Task<Conversation> CreateConversation(string name, Guid userId);
}