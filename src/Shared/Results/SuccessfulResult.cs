namespace Shared.Results;

/// <summary>
/// Represents a successful operation that produced a value.
/// </summary>
/// <typeparam name="TValue">The type of value produced by the operation.</typeparam>
internal sealed class SuccessfulResult<TValue> : Result<TValue>
{
    public override TValue Value { get; }

    /// <summary>
    /// Initializes a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value produced by the operation.</param>
    internal SuccessfulResult(TValue value)
        => Value = value;
}
