using ChatBot.ChatHistory;
using ChatBot.ChatProviders;

namespace ChatBot.Chat;

public class ChatService(IChatProvider chatProvider, IChatHistory chatHistory)
{
    public async Task<Message> SendMessageAsync(Message message, Guid conversationId)
    {
        var addMessageTask = chatHistory.AddMessageAsync(message, conversationId);

        var chatResponseTask = chatProvider.SendMessageAsync(message);

        await Task.WhenAll(addMessageTask, chatResponseTask);

        await chatHistory.AddMessageAsync(chatResponseTask.Result, conversationId);

        return chatResponseTask.Result;
    }

    public async Task<IEnumerable<Message>> GetMessages(Guid conversationId)
    {
        return await chatHistory.GetMessages(conversationId);
    }
}