using ChatBot.ChatHistory;
using ChatBot.ChatProviders;

namespace ChatBot.ChatService;

public class ChatService(IChatProvider chatProvider, IChatHistory chatHistory)
{
    public async Task<Message> SendMessageAsync(Message message, Guid conversationId)
    {
        var addMessageTask = chatHistory.AddMessageAsync(message, MessageAuthor.User, conversationId);
        
        var chatResponseTask = chatProvider.SendMessageAsync(message);
        
        await Task.WhenAll(addMessageTask, chatResponseTask);

        await chatHistory.AddMessageAsync(chatResponseTask.Result, MessageAuthor.Bot, conversationId);

        return chatResponseTask.Result;
    }
}