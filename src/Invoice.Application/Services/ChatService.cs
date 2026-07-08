using System.Text.Json;
using System.Text.Json.Serialization;

namespace Invoice.Application.Services;

public class ChatService(
    IAiChatClient aiClient,
    IInvoiceService invoiceService) : IChatService
{
    private const int MaxToolIterations = 5;
    private const string FinalAnswerToolName = "final_answer";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly string[] InvoiceStatusNames = Enum.GetNames<InvoiceStatus>();

    private const string SystemPrompt = """
        Ты — ИИ-ассистент, встроенный в систему управления счетами (Invoice Management).
        Твоя единственная задача — помогать текущему пользователю находить и просматривать ЕГО счета в этом приложении,
        чтобы он мог скачать нужный PDF. Ты НЕ умеешь и НЕ должен создавать, изменять, удалять или отправлять счета,
        а также не работаешь с клиентами, услугами или любыми другими сущностями — только просмотр счетов.

        Правила:
        - Отвечай ТОЛЬКО на вопросы о просмотре счетов пользователя (список, статус, сумма, детали конкретного счёта).
        - Если пользователь просит создать, изменить, удалить, отправить счёт или что-то сделать с клиентами —
          вежливо объясни, что ты умеешь только показывать существующие счета, и предложи сделать это через интерфейс приложения.
        - Если вопрос не связан со счетами этого приложения (погода, новости, отвлечённые темы и т.п.) — вежливо откажись
          и предложи задать вопрос по счетам.
        - Никогда не выдумывай цифры и факты. Для любых данных пользователя используй только предоставленные инструменты.
        - Если упоминаешь конкретный счёт, сначала узнай его id через инструмент get_invoice/list_invoices
          и добавь этот id в invoiceIds финального ответа, чтобы пользователь мог скачать его PDF.
        - Отвечай на языке, на котором пишет пользователь.
        - Всегда завершай ответ вызовом функции final_answer — никогда не отвечай простым текстом.
        """;

    public async Task<ResponseModel<ChatResponse>> SendMessageAsync(Guid ownerUserId, ChatRequest request)
    {
        var messages = new List<AiMessage> { AiMessage.System(SystemPrompt) };
        messages.AddRange(request.History.Select(h =>
            h.Role == "assistant" ? AiMessage.Assistant(h.Content) : AiMessage.User(h.Content)));
        messages.Add(AiMessage.User(request.Message));

        var tools = BuildTools();

        for (var i = 0; i < MaxToolIterations; i++)
        {
            var forceToolName = i == MaxToolIterations - 1 ? FinalAnswerToolName : null;
            var assistantMessage = await aiClient.GetCompletionAsync(messages, tools, forceToolName);
            messages.Add(assistantMessage);

            if (assistantMessage.ToolCalls is not { Count: > 0 })
            {
                return ResponseModel.Success(new ChatResponse
                {
                    Reply = assistantMessage.Content ?? string.Empty,
                    Suggestions = [],
                    InvoiceIds = []
                });
            }

            foreach (var call in assistantMessage.ToolCalls)
            {
                if (call.FunctionName == FinalAnswerToolName)
                {
                    return ResponseModel.Success(ParseFinalAnswer(call.ArgumentsJson));
                }

                var resultJson = await ExecuteToolAsync(ownerUserId, call.FunctionName, call.ArgumentsJson);
                messages.Add(AiMessage.ToolResult(call.Id, resultJson));
            }
        }

        return ResponseModel.Failure<ChatResponse>("Не удалось получить ответ от ассистента.", 502);
    }

    private async Task<string> ExecuteToolAsync(Guid ownerUserId, string name, string argumentsJson)
    {
        try
        {
            switch (name)
            {
                case "list_invoices":
                {
                    var args = JsonSerializer.Deserialize<ListInvoicesArgs>(argumentsJson, JsonOptions) ?? new ListInvoicesArgs();
                    var result = await invoiceService.GetListAsync(
                        ownerUserId,
                        args.PageNumber is null or <= 0 ? 1 : args.PageNumber.Value,
                        args.PageSize is null or <= 0 ? 10 : Math.Min(args.PageSize.Value, 20),
                        null,
                        args.Status,
                        null,
                        false);
                    return Serialize(result);
                }
                case "get_invoice":
                {
                    var args = JsonSerializer.Deserialize<GetInvoiceArgs>(argumentsJson, JsonOptions);
                    if (args is null || !Guid.TryParse(args.InvoiceId, out var invoiceId))
                    {
                        return JsonSerializer.Serialize(new { error = "invoiceId должен быть корректным GUID." });
                    }

                    var result = await invoiceService.GetByIdAsync(ownerUserId, invoiceId);
                    return Serialize(result);
                }
                default:
                    return JsonSerializer.Serialize(new { error = $"Неизвестный инструмент: {name}" });
            }
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(new { error = "Некорректные аргументы инструмента." });
        }
    }

    private static string Serialize<T>(ResponseModel<T> result) =>
        JsonSerializer.Serialize(result.IsSucceeded ? (object)result.Data! : new { error = result.Message }, JsonOptions);

    private static ChatResponse ParseFinalAnswer(string argumentsJson)
    {
        FinalAnswerArgs? args;
        try
        {
            args = JsonSerializer.Deserialize<FinalAnswerArgs>(argumentsJson, JsonOptions);
        }
        catch (JsonException)
        {
            args = null;
        }

        args ??= new FinalAnswerArgs();

        return new ChatResponse
        {
            Reply = string.IsNullOrWhiteSpace(args.Reply)
                ? "Не удалось сформировать ответ, попробуйте переформулировать вопрос."
                : args.Reply,
            Suggestions = args.Suggestions?.Where(s => !string.IsNullOrWhiteSpace(s)).Take(4).ToList() ?? [],
            InvoiceIds = args.InvoiceIds?
                .Select(id => Guid.TryParse(id, out var guid) ? guid : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .ToList() ?? []
        };
    }

    private static List<AiTool> BuildTools() =>
    [
        new AiTool
        {
            Name = "list_invoices",
            Description = "Возвращает список счетов пользователя с необязательным фильтром по статусу.",
            ParametersSchema = new
            {
                type = "object",
                properties = new
                {
                    status = new { type = "string", @enum = InvoiceStatusNames, description = "Необязательный фильтр по статусу" },
                    pageNumber = new { type = "integer", description = "Номер страницы, по умолчанию 1" },
                    pageSize = new { type = "integer", description = "Размер страницы, по умолчанию 10, максимум 20" }
                }
            }
        },
        new AiTool
        {
            Name = "get_invoice",
            Description = "Возвращает подробную информацию об одном счёте пользователя по его id.",
            ParametersSchema = new
            {
                type = "object",
                properties = new
                {
                    invoiceId = new { type = "string", description = "Id счёта (GUID)" }
                },
                required = new[] { "invoiceId" }
            }
        },
        new AiTool
        {
            Name = FinalAnswerToolName,
            Description = "Формирует финальный структурированный ответ пользователю. Всегда вызывай эту функцию последней.",
            ParametersSchema = new
            {
                type = "object",
                properties = new
                {
                    reply = new { type = "string", description = "Текст ответа пользователю" },
                    suggestions = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "2-4 коротких варианта следующего вопроса пользователя"
                    },
                    invoiceIds = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Id счетов (GUID), упомянутых в ответе, для кнопки скачивания PDF"
                    }
                },
                required = new[] { "reply" }
            }
        }
    ];

    private sealed class ListInvoicesArgs
    {
        public InvoiceStatus? Status { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }

    private sealed class GetInvoiceArgs
    {
        public string? InvoiceId { get; set; }
    }

    private sealed class FinalAnswerArgs
    {
        public string? Reply { get; set; }
        public List<string>? Suggestions { get; set; }
        public List<string>? InvoiceIds { get; set; }
    }
}
