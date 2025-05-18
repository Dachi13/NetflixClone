namespace Shared.Library;

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue value) => _value = value;

    protected internal Result(Error error) : base(error)
    {
        _value = default;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public TResult Match<TResult>(
        Func<TValue, TResult> success,
        Func<Error, TResult> failure)
        => IsSuccess
            ? success(Value)
            : failure(Error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}