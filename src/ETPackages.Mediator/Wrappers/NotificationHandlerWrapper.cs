using System.Collections.Concurrent;
using ETPackages.Mediator.Abstractions;
using ETPackages.Mediator.Contracts;

namespace ETPackages.Mediator.Wrappers
{
    public abstract class NotificationHandlerWrapper
    {
        private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDictionary = new ConcurrentDictionary<Type, Type>();

        public abstract Task Handle(INotification notification, CancellationToken cancellationToken);

        public static NotificationHandlerWrapper Create(object handler, Type notificationType)
        {
            Type notificationWrapperType = WrapperTypeDictionary.GetOrAdd(
                notificationType,
                nt => typeof(NotificationHandlerWrapper<>).MakeGenericType(nt));

            return (NotificationHandlerWrapper)Activator.CreateInstance(notificationWrapperType, handler)!;
        }
    }

    internal sealed class NotificationHandlerWrapper<TNotification>(object handler) : NotificationHandlerWrapper 
        where TNotification : INotification
    {
        private readonly INotificationHandler<TNotification> _handler = (INotificationHandler<TNotification>)handler;

        public override async Task Handle(INotification notification, CancellationToken cancellationToken)
        {
            await _handler.Handle((TNotification)notification, cancellationToken);
        }
    }
}
