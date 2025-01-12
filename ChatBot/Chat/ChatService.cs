using ChatBot.ChatHistory;
using ChatBot.ChatProviders;

namespace ChatBot.Chat;

public class ChatService(IChatProvider chatProvider, IChatHistory chatHistory)
{
    public async Task<Message> SendMessageAsync(Message message, Guid conversationId)
    {
        var messages = (await GetMessages(conversationId)).ToList();
        messages.Add(message);

        var addMessageTask = chatHistory.AddMessageAsync(message, conversationId);

        var chatResponseTask = chatProvider.SendMessagesAsync(messages);

        await Task.WhenAll(addMessageTask, chatResponseTask);

        await chatHistory.AddMessageAsync(chatResponseTask.Result, conversationId);

        return chatResponseTask.Result;
    }

    public async Task<IEnumerable<Message>> GetMessages(Guid conversationId)
    {
        return await chatHistory.GetMessages(conversationId);
    }

    public async Task<IEnumerable<Conversation>> GetConversations(Guid userId)
    {
        return await chatHistory.GetConversations(userId);
    }

    public async Task<Conversation> CreateConversation(string name, Guid userId)
    {
        return await chatHistory.CreateConversation(name, userId);
    }
}