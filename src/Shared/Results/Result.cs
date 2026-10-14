namespace Shared.Results;

/// <summary>
/// Represents the outcome of an operation that produces a value when successful.
/// </summary>
/// <typeparam name="TValue">The type of value produced by the operation.</typeparam>
public abstract class Result<TValue> : Result
{
    /// <summary>
    /// Gets the value produced by a successful operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result represents a failed operation.
    /// </exception>
    public abstract TValue Value { get; }

    /// <summary>
    /// Initializes a successful result.
    /// </summary>
    private protected Result() { }

    /// <summary>
    /// Initializes a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the operation to fail.</param>
    private protected Result(Error error) : base(error) { }
}

/// <summary>
/// Represents the outcome of an operation that does not produce a value.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error associated with the operation.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Initializes a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the operation to fail.</param>
    internal Result(Error error)
    {
        IsSuccess = false;
        Error = error;
    }

    /// <summary>
    /// Initializes a successful result.
    /// </summary>
    internal Result()
    {
        IsSuccess = true;
        Error = Error.None;
    }
}
