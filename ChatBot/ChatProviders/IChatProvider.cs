using ChatBot.Chat;

namespace ChatBot.ChatProviders;

public interface IChatProvider
{
    Task<Message> SendMessagesAsync(IEnumerable<Message> messages);
}