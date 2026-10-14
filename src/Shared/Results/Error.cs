namespace Shared.Results;
public abstract record Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    public static Error None { get; } = new NoError();

    public Error(ErrorType errorType, string code, string message)
    {
        Type = errorType;
        Code = code;
        Message = message;
    }
}

public enum ErrorType
{
    None = 0,
    Exception = 1,
    Failure = 2,
    Validation = 3,
    NotFound = 4,
    Conflict = 5
}