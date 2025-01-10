namespace ChatBot.ChatHistory;

public interface IChatHistory
{
    public Task AddMessageAsync(Message message);
    
    public IEnumerable<string> GetMessages(Guid conversationId);
}