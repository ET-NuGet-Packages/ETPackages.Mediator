using ETPackages.Mediator.Abstractions;
using ETPackages.Mediator.Contracts;
using ETPackages.Mediator.Wrappers;

namespace ETPackages.Mediator.NotificationPublishers
{
    public class TaskWhenAllPublisher : INotificationPublisher
    {
        public async Task Publish(IEnumerable<object?> handlers, INotification notification, CancellationToken cancellationToken)
        {
            Task[] notificationTasks = handlers
                .Select(handler =>
                {
                    NotificationHandlerWrapper handlerWrapper = NotificationHandlerWrapper.Create(handler, notification.GetType());

                    return handlerWrapper.Handle(notification, cancellationToken);
                })
                .ToArray();

            await Task.WhenAll(notificationTasks);
        }
    }
}
