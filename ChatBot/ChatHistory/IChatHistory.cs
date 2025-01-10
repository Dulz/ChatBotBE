using ChatBot.ChatService;

namespace ChatBot.ChatHistory;

public interface IChatHistory
{
    public Task AddMessageAsync(Message message, MessageAuthor messageAuthor, Guid conversationId);
    
    public IEnumerable<string> GetMessages(Guid conversationId);
}