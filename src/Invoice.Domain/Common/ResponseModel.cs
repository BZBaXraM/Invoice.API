namespace Invoice.Domain.Common;

public class ResponseModel
{
    public string Message { get; set; }
    public bool IsSucceeded { get; set; }
    public bool IsFailed => !IsSucceeded;
    public int StatusCode { get; set; }
    public List<string>? Errors { get; set; }

    public ResponseModel()
    {
        StatusCode = 0;
        IsSucceeded = false;
        Message = string.Empty;
    }

    public ResponseModel(bool isSucceeded, string message, int statusCode, List<string>? errors = null)
    {
        IsSucceeded = isSucceeded;
        Message = message;
        StatusCode = statusCode;
        Errors = errors;
    }

    public static ResponseModel Success() => new(true, string.Empty, 200);
    public static ResponseModel Success(string message) => new(true, message, 200);
    public static ResponseModel Failure(string message) => new(false, message, 400);
    public static ResponseModel Failure(string message, int statusCode) => new(false, message, statusCode);
    public static ResponseModel Failure(string message, List<string>? errors) => new(false, message, 400, errors);

    public static ResponseModel Failure(string message, int statusCode, List<string>? errors) =>
        new(false, message, statusCode, errors);

    public static ResponseModel<T> Success<T>(T data) => new(data, true, string.Empty, 200);
    public static ResponseModel<T> Success<T>(T data, string message) => new(data, true, message, 200);
    public static ResponseModel<T> Failure<T>(string message) => new(default, false, message, 400);
    public static ResponseModel<T> Failure<T>(string message, int statusCode) => new(default, false, message, statusCode);

    public static ResponseModel<T> Failure<T>(string message, List<string>? errors) =>
        new(default, false, message, 400, errors);

    public static ResponseModel<T> Failure<T>(string message, int statusCode, List<string>? errors) =>
        new(default, false, message, statusCode, errors);

    public static ResponseModel<T> Failure<T>(T? data, string message, int statusCode, List<string>? errors = null) =>
        new(data, false, message, statusCode, errors);
}

public class ResponseModel<T> : ResponseModel
{
    public T? Data { get; set; }

    public ResponseModel() : base()
    {
    }

    public ResponseModel(T? data, bool isSucceeded, string message, int statusCode, List<string>? errors = null)
        : base(isSucceeded, message, statusCode, errors)
    {
        Data = data;
    }
}
