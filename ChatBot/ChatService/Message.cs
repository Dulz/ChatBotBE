namespace ChatBot.ChatService;

public class Message(string content)
{
    public string Content { get; private set; } = content;
}