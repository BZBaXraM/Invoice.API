namespace Invoice.Application.ServiceContracts;

/// <summary>
/// A thin, provider-agnostic chat-completion client with tool/function calling support.
/// </summary>
public interface IAiChatClient
{
    /// <summary>
    /// Requests the next assistant message given the conversation so far and the tools it may call.
    /// </summary>
    /// <param name="tools"></param>
    /// <param name="forceToolName">
    /// When set, forces the model to call this specific tool instead of choosing freely.
    /// </param>
    /// <param name="messages"></param>
    /// <param name="cancellationToken"></param>
    Task<AiMessage> GetCompletionAsync(
        IReadOnlyList<AiMessage> messages,
        IReadOnlyList<AiTool> tools,
        string? forceToolName,
        CancellationToken cancellationToken = default);
}
