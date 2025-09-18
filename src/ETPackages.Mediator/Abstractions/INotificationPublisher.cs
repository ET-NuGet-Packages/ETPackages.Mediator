using ETPackages.Mediator.Contracts;

namespace ETPackages.Mediator.Abstractions
{
    public interface INotificationPublisher
    {
        Task Publish(IEnumerable<object?> handlers, INotification notification, CancellationToken cancellationToken);
    }
}
