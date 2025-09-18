using ETPackages.Mediator.Abstractions;
using ETPackages.Mediator.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ETPackages.Mediator
{
    public static class ServiceRegistrar
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorServiceConfiguration> configureOptions)
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var config = new MediatorServiceConfiguration();

            configureOptions(config);

            services.TryAdd(new ServiceDescriptor(typeof(INotificationPublisher), config.NotificationPublisher.GetType(), config.Lifetime));

            services.TryAdd(new ServiceDescriptor(typeof(IMediator), typeof(Mediator), config.Lifetime));

            foreach (var assembly in config.Assemblies)
            {
                IEnumerable<Type> types = assembly.GetTypes().Where(t => !t.IsInterface && !t.IsAbstract);

                var handlerTypes = types.SelectMany(t => t
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && (
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) || 
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
                    ))
                    .Select(s => new
                    {
                        Interface = s,
                        Implementation = t
                    }));

                foreach (var item in handlerTypes)
                {
                    if (item.Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                        item.Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    {
                        services.TryAdd(new ServiceDescriptor(item.Interface, item.Implementation, config.Lifetime));
                    }
                    else
                    {
                        services.TryAddEnumerable(new ServiceDescriptor(item.Interface, item.Implementation, config.Lifetime));
                    }
                }
            }

            foreach (var pipeline in config.PipelineBehaviors)
            {
                var genericArg = pipeline.GetGenericArguments().Length;

                Type pipelineTypeInterface = genericArg switch
                {
                    1 => typeof(IPipelineBehavior<>),
                    2 => typeof(IPipelineBehavior<,>),
                    _ => throw new ArgumentOutOfRangeException(nameof(genericArg))
                };

                services.TryAddEnumerable(new ServiceDescriptor(pipelineTypeInterface, pipeline, config.Lifetime));
            }

            return services;
        }
    }
}
