using ETPackages.Mediator.Contracts;

namespace ETPackages.Mediator.Abstractions
{
    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        Task Send(IRequest request, CancellationToken cancellationToken = default);

        Task Publish(INotification notification, CancellationToken cancellationToken = default);
    }
}
