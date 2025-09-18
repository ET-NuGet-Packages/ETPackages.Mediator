using ETPackages.Mediator.Contracts;

namespace ETPackages.Mediator.Abstractions
{
    public interface INotificationHandler<TNotification> where TNotification : INotification
    {
        Task Handle(TNotification notification, CancellationToken cancellationToken = default);
    }
}
