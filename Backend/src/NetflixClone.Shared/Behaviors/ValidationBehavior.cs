using FluentValidation;
using MediatR;
using Shared.Library;
using Shared.Models;

namespace Shared.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next();

        Error[] errors = validators
            .Select(validator => validator.Validate(request))
            .SelectMany(validationResult => validationResult.Errors)
            .Where(validationFailure => validationFailure is not null)
            .Select(failure => new Error(
                    failure.PropertyName,
                    failure.ErrorMessage,
                    ErrorType.UnprocessableEntity
                )
            )
            .Distinct()
            .ToArray();

        if (errors.Any()) return CreateValidationResult<TResponse>(errors[0]);

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(Error errors)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result)) return (Result.Failure(errors) as TResult)!;

        var result = typeof(Result)
            .GetMethods()
            .First(m =>
                m is { IsGenericMethod: true, Name: nameof(Result.Failure) } &&
                m.GetParameters().First().ParameterType == typeof(Error))!
            .MakeGenericMethod(typeof(TResult).GenericTypeArguments[0])
            .Invoke(null, new object?[] { errors })!;

        return (TResult)result;
    }
}