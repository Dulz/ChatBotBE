using ChatBot.ChatService;

namespace ChatBot.ChatProviders;

public interface IChatProvider
{
    Task<Message> SendMessageAsync(Message message);
}