namespace Shared.Results;

/// <summary>
/// Represents a failed operation that did not produce a value.
/// </summary>
/// <typeparam name="TValue">The value type expected from the operation.</typeparam>
internal sealed class FailedResult<TValue> : Result<TValue>
{
    /// <inheritdoc />
    public override TValue Value =>
        throw new InvalidOperationException(
            "A failed result does not contain a value.");

    /// <summary>
    /// Initializes a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the operation to fail.</param>
    internal FailedResult(Error error) : base(error) { }
}
