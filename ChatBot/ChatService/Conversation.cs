namespace ChatBot.ChatService;

public class Conversation(Guid id, string name)
{
    public Guid Id { get; private set; } = id;
    public string Name { get; private set; } = name;
}