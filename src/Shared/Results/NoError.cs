namespace Shared.Results;

public sealed record NoError : Error
{
    internal NoError() : base(
        ErrorType.None, 
        string.Empty, 
        string.Empty) { }
}
