namespace Shared.Results;

/// <summary>
/// Creates successful and failed operation results.
/// </summary>
public static class ResultFactory
{
    /// <summary>
    /// Creates a successful result without a value.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success() => new Result();

    /// <summary>
    /// Creates a failed result without a value.
    /// </summary>
    /// <param name="error">The error that caused the operation to fail.</param>
    /// <returns>A failed result containing the specified error.</returns>
    public static Result Failure(Error error) => new Result(error);

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value produced by the operation.</param>
    /// <returns>A successful result containing the specified value.</returns>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new SuccessfulResult<TValue>(value);

    /// <summary>
    /// Creates a failed result for the specified value type.
    /// </summary>
    /// <typeparam name="TValue">The value type expected from the operation.</typeparam>
    /// <param name="error">The error that caused the operation to fail.</param>
    /// <returns>A failed result containing the specified error.</returns>
    public static Result<TValue> Failure<TValue>(Error error) =>
        new FailedResult<TValue>(error);
}
