using ChatBot.Chat;

namespace ChatBot.ChatHistory;

public interface IChatHistory
{
    public Task AddMessageAsync(Message message, Guid conversationId);

    public Task<IEnumerable<Message>> GetMessages(Guid conversationId);
}