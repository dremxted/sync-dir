namespace Shared.Results;

/// <summary>
/// Provides operations for matching results and creating specialized errors.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a result without a value to an output value by invoking the callback
    /// associated with its state.
    /// </summary>
    /// <typeparam name="TOut">The type of value returned by either callback.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">The callback invoked when the result is successful.</param>
    /// <param name="onFailure">The callback invoked with the error when the result failed.</param>
    /// <returns>The value returned by the callback associated with the result state.</returns>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure) =>
        result.IsSuccess
            ? onSuccess()
            : onFailure(result.Error);

    /// <summary>
    /// Maps a result containing a value to an output value by invoking the callback
    /// associated with its state.
    /// </summary>
    /// <typeparam name="TValue">The type of value produced by a successful result.</typeparam>
    /// <typeparam name="TOut">The type of value returned by either callback.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">
    /// The callback invoked with the result value when the result is successful.
    /// </param>
    /// <param name="onFailure">The callback invoked with the error when the result failed.</param>
    /// <returns>The value returned by the callback associated with the result state.</returns>
    public static TOut Match<TValue, TOut>(
        this Result<TValue> result,
        Func<TValue, TOut> onSuccess,
        Func<Error, TOut> onFailure) =>
        result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);

    /// <summary>
    /// Creates an exception error with the specified code and message.
    /// </summary>
    /// <param name="error">The error instance used to invoke the extension method.</param>
    /// <param name="code">The application-specific error code.</param>
    /// <param name="message">The human-readable error message.</param>
    /// <returns>An exception error containing the specified code and message.</returns>

    /// <summary>
    /// Maps a successful result value to another type. 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="result"></param>
    /// <param name="selector"></param>
    /// <returns>
    /// A successful result containing the mapped value, or the original error if the result failed.
    /// </returns>
    public static Result<TOut> Map<TValue,TOut>(
        this Result<TValue> result,
        Func<TValue, TOut> selector)
    {
        if(result.IsFailure)
        {
            return ResultFactory
                .Failure<TOut>(result.Error);
        }

        return ResultFactory
            .Success(selector(result.Value));
    }
}
