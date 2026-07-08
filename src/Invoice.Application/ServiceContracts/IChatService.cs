namespace Invoice.Application.ServiceContracts;

public interface IChatService
{
    Task<ResponseModel<ChatResponse>> SendMessageAsync(Guid ownerUserId, ChatRequest request);
}
