namespace ChatBot.ChatProviders;

public interface IChatProvider
{
    Task<string> SendMessageAsync(string message);
}