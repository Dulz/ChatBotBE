using ChatBot.Chat;

namespace ChatBot.ChatHistory;

public interface IChatHistory
{
    public Task AddMessageAsync(Message message, Guid conversationId);

    public Task<IEnumerable<Message>> GetMessages(Guid conversationId);

    Task<IEnumerable<Conversation>> GetConversations(Guid userId);

    Task<Conversation> CreateConversation(string name, Guid userId);
    
    Task<bool> UserHasConversation(Guid userId, Guid conversationId);
}