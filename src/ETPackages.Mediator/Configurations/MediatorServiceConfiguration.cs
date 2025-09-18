using System.Reflection;
using ETPackages.Mediator.Abstractions;
using ETPackages.Mediator.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;

namespace ETPackages.Mediator.Configurations
{
    public class MediatorServiceConfiguration
    {
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
        public INotificationPublisher NotificationPublisher { get; set; } = new ForeachAwaitPublisher();
        internal List<Assembly> Assemblies { get; set; } = new List<Assembly>();
        internal List<Type> PipelineBehaviors { get; set; } = new List<Type>();

        public MediatorServiceConfiguration AddRegisterAssembly(Assembly assembly)
        {
            Assemblies.Add(assembly);

            return this;
        }

        public MediatorServiceConfiguration AddRegisterAssemblies(params Assembly[] assemblies)
        {
            Assemblies.AddRange(assemblies);

            return this;
        }

        public void AddOpenBehavior(Type behaviorType)
        {
            PipelineBehaviors.Add(behaviorType);
        }
    }
}
