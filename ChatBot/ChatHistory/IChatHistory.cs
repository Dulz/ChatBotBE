using ChatBot.ChatService;

namespace ChatBot.ChatHistory;

public interface IChatHistory
{
    public Task AddMessageAsync(Message message, MessageAuthor messageAuthor, Conversation conversation);
    
    public IEnumerable<string> GetMessages(Guid conversationId);
}