using System.Collections.Concurrent;
using ETPackages.Mediator.Abstractions;
using ETPackages.Mediator.Contracts;
using ETPackages.Mediator.Wrappers;
using Microsoft.Extensions.DependencyInjection;

namespace ETPackages.Mediator
{
    internal class Mediator(IServiceProvider serviceProvider, INotificationPublisher notificationPublisher) : IMediator
    {
        private static readonly ConcurrentDictionary<Type, Type> RequestHandlerTypeDictionary = new ConcurrentDictionary<Type, Type>();
        private static readonly ConcurrentDictionary<Type, Type> NotificationHandlerTypeDictionary = new ConcurrentDictionary<Type, Type>();

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using var scope = serviceProvider.CreateScope();

            Type requestType = request.GetType();

            Type interfaceType = RequestHandlerTypeDictionary.GetOrAdd(
                requestType,
                rt => typeof(IRequestHandler<,>).MakeGenericType(rt, typeof(TResponse)));

            var handler = scope.ServiceProvider.GetRequiredService(interfaceType);

            RequestHandlerWrapper<TResponse> requesthandlerWrapper = RequestHandlerWrapper<TResponse>.Create(handler, requestType);

            return await requesthandlerWrapper.Handle(request, serviceProvider, cancellationToken);
        }

        public async Task Send(IRequest request, CancellationToken cancellationToken = default) 
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using var scope = serviceProvider.CreateScope();

            Type requestType = request.GetType();

            Type interfaceType = RequestHandlerTypeDictionary.GetOrAdd(
                requestType,
                rt => typeof(IRequestHandler<>).MakeGenericType(rt));

            var handler = scope.ServiceProvider.GetRequiredService(interfaceType);

            RequestHandlerWrapper requesthandlerWrapper = RequestHandlerWrapper.Create(handler, requestType);

            await requesthandlerWrapper.Handle(request, serviceProvider, cancellationToken);
        }
        
        public async Task Publish(INotification notification, CancellationToken cancellationToken = default) 
        {
            using var scope = serviceProvider.CreateScope();

            Type notificationType = notification.GetType();

            Type interfaceType = NotificationHandlerTypeDictionary.GetOrAdd(
                notificationType,
                nt => typeof(INotificationHandler<>).MakeGenericType(nt));

            IEnumerable<object?> notificationHandlers = scope.ServiceProvider.GetServices(interfaceType);

            await notificationPublisher.Publish(notificationHandlers, notification, cancellationToken);
        }
    }
}
