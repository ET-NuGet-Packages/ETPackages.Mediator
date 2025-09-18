using System.Collections.Concurrent;
using ETPackages.Mediator.Abstractions;
using ETPackages.Mediator.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ETPackages.Mediator.Wrappers
{
    public class RequestHandlerWrapperBase
    {
        protected static readonly ConcurrentDictionary<Type, Type> WrapperTypeDictionary = new ConcurrentDictionary<Type, Type>();
    }

    public abstract class RequestHandlerWrapper : RequestHandlerWrapperBase
    {
        public abstract Task Handle(IRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken);

        public static RequestHandlerWrapper Create(object handler, Type requestType)
        {
            Type requestWrapperType = WrapperTypeDictionary.GetOrAdd(
                requestType,
                rt => typeof(RequestHandlerWrapperImplementation<>).MakeGenericType(rt));

            return (RequestHandlerWrapper)Activator.CreateInstance(requestWrapperType, handler)!;
        }
    }

    public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerWrapperBase
    {
        public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken);

        public static RequestHandlerWrapper<TResponse> Create(object handler, Type requestType)
        {
            Type requestWrapperType = WrapperTypeDictionary.GetOrAdd(
                requestType,
                rt => typeof(RequestHandlerWrapperImplementation<,>).MakeGenericType(rt, typeof(TResponse)));

            return (RequestHandlerWrapper<TResponse>)Activator.CreateInstance(requestWrapperType, handler)!;
        }
    }

    internal sealed class RequestHandlerWrapperImplementation<TRequest>(object handler) : RequestHandlerWrapper 
        where TRequest : IRequest
    {
        private readonly IRequestHandler<TRequest> _handler = (IRequestHandler<TRequest>)handler;

        public override async Task Handle(IRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            RequestHandlerDelegate handlerDelegate = () =>
            {
                return _handler.Handle((TRequest)request, cancellationToken);
            };

            var pipeline = serviceProvider
                .GetServices<IPipelineBehavior<TRequest>>()
                .Reverse()
                .Aggregate(handlerDelegate, (next, behavior) =>
                {
                    return () =>
                    {
                        return behavior.Handle((TRequest)request, next, cancellationToken);
                    };
                });

            await pipeline();
        }
    }

    internal sealed class RequestHandlerWrapperImplementation<TRequest, TResponse>(object handler) : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _handler = (IRequestHandler<TRequest, TResponse>)handler;

        public override async Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            RequestHandlerDelegate<TResponse> handlerDelegate = () =>
            {
                return _handler.Handle((TRequest)request, cancellationToken);
            };

            var pipeline = serviceProvider
                .GetServices<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse()
                .Aggregate(handlerDelegate, (next, behavior) =>
                {
                    return () =>
                    {
                        return behavior.Handle((TRequest)request, next, cancellationToken);
                    };
                });

            return await pipeline();
        }
    }
}
