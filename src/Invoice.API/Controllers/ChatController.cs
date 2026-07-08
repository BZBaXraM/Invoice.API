namespace Invoice.API.Controllers;

/// <summary>
/// Controller for the AI invoice-viewing chat.
/// </summary>
[Authorize]
[Route("api/chat")]
[ApiController]
public class ChatController(IChatService service, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Sends a message to the AI invoice chat and gets back a structured reply.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResponseModel<ChatResponse>>> SendMessage([FromBody] ChatRequest request)
    {
        var result = await service.SendMessageAsync(currentUserService.UserId!.Value, request);
        return StatusCode(result.StatusCode, result);
    }
}
