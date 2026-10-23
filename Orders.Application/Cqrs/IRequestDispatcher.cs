namespace Orders.Application.Cqrs;

public interface IRequestDispatcher
{
    Task<TResponse> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;
}
