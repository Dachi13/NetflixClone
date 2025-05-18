using MediatR;
using Shared.Library;

namespace Shared.CQRS;

public interface IQuery<TResponse> : IRequest <Result<TResponse>> where TResponse : notnull
{
    
}