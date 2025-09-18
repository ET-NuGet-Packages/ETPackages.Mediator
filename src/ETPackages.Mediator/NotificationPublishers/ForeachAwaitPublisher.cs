using ETPackages.Mediator.Abstractions;
using ETPackages.Mediator.Contracts;
using ETPackages.Mediator.Wrappers;

namespace ETPackages.Mediator.NotificationPublishers
{
    public class ForeachAwaitPublisher : INotificationPublisher
    {
        public async Task Publish(IEnumerable<object?> handlers, INotification notification, CancellationToken cancellationToken)
        {
            foreach (object? handler in handlers)
            {
                if (handler == null)
                {
                    continue;
                }

                NotificationHandlerWrapper handlerWrapper = NotificationHandlerWrapper.Create(handler, notification.GetType());

                await handlerWrapper.Handle(notification, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
