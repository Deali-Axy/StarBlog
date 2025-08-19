using System.ClientModel;
using DataProc.Entities;
using FluentResults;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;

namespace DataProc.Services;

public class LLM : IService {
    private IChatClient _chatClient;

    public LLM(IOptions<AppSettings> settings) {
        var llmConfig = settings.Value.LLM;
        _chatClient = new OpenAIClient(
            new ApiKeyCredential(llmConfig.Key),
            new OpenAIClientOptions {
                Endpoint = new Uri(llmConfig.Endpoint)
            }
        ).GetChatClient(llmConfig.Model).AsIChatClient();
    }

    public IChatClient ChatClient { get; set; }

    public Task<Result> Run() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 生成文本
    /// </summary>
    /// <param name="prompt">提示词</param>
    /// <returns>生成的文本</returns>
    public async Task<string> GenerateTextAsync(string prompt) {
        try {
            var response = await ChatClient.GetResponseAsync(prompt);
            return response.Text;
        }
        catch (Exception ex) {
            throw new Exception($"AI文本生成失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 生成聊天回复
    /// </summary>
    /// <param name="messages">聊天历史记录</param>
    /// <returns>AI的回复</returns>
    public async Task<string> GenerateChatReplyAsync(params ChatMessage[] messages) {
        try {
            var response = await ChatClient.GetResponseAsync(messages);
            return response.Text;
        }
        catch (Exception ex) {
            throw new Exception($"AI聊天回复生成失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 流式生成文本
    /// </summary>
    /// <param name="prompt">提示词</param>
    /// <returns>生成的文本流</returns>
    public IAsyncEnumerable<ChatResponseUpdate> GenerateTextStreamAsync(string prompt) {
        try {
            return ChatClient.GetStreamingResponseAsync(prompt);
        }
        catch (Exception ex) {
            throw new Exception($"AI文本流生成失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 流式生成聊天回复
    /// </summary>
    /// <param name="messages">聊天历史记录</param>
    /// <returns>AI的回复流</returns>
    public IAsyncEnumerable<ChatResponseUpdate> GenerateChatReplyStreamAsync(params ChatMessage[] messages) {
        try {
            return ChatClient.GetStreamingResponseAsync(messages);
        }
        catch (Exception ex) {
            throw new Exception($"AI聊天回复流生成失败: {ex.Message}", ex);
        }
    }
}