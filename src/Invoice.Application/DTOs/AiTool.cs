namespace Invoice.Application.DTOs;

/// <summary>
/// A function tool definition offered to the AI model, described as a JSON schema.
/// </summary>
public class AiTool
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required object ParametersSchema { get; set; }
}

/// <summary>
/// A function call the model requested, with its arguments as a raw JSON string.
/// </summary>
public class AiToolCall
{
    public required string Id { get; set; }
    public required string FunctionName { get; set; }
    public required string ArgumentsJson { get; set; }
}
