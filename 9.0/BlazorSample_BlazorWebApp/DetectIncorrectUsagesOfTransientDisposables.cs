using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    using BlazorServerTransientDisposable;

    public static class WebHostBuilderTransientDisposableExtensions
    {
        public static WebApplicationBuilder DetectIncorrectUsageOfTransients(
            this WebApplicationBuilder builder)
        {
            builder.Host
                .UseServiceProviderFactory(
                    new DetectIncorrectUsageOfTransientDisposablesServiceFactory())
                .ConfigureServices(
                    s => s.TryAddEnumerable(ServiceDescriptor.Scoped<CircuitHandler,
                        ThrowOnTransientDisposableHandler>()));

            return builder;
        }
    }
}

namespace BlazorServerTransientDisposable
{
    internal class ThrowOnTransientDisposableHandler : CircuitHandler
    {
        public ThrowOnTransientDisposableHandler(
            ThrowOnTransientDisposable throwOnTransientDisposable)
        {
            throwOnTransientDisposable.ShouldThrow = true;
        }
    }

    public class DetectIncorrectUsageOfTransientDisposablesServiceFactory 
        : IServiceProviderFactory<IServiceCollection>
    {
        public IServiceCollection CreateBuilder(IServiceCollection services) => 
            services;

        public IServiceProvider CreateServiceProvider(
            IServiceCollection containerBuilder)
        {
            var collection = new ServiceCollection();

            foreach (var descriptor in containerBuilder)
            {
                switch (descriptor.Lifetime)
                {
                    case ServiceLifetime.Transient 
                        when (descriptor is { IsKeyedService: true, KeyedImplementationType: not null }
                            && typeof(IDisposable).IsAssignableFrom(descriptor.KeyedImplementationType))
                            || (descriptor is { IsKeyedService: false, ImplementationType: not null }
                                && typeof(IDisposable).IsAssignableFrom(descriptor.ImplementationType)):
                        collection.Add(CreatePatchedDescriptor(descriptor));
                        break;
                    case ServiceLifetime.Transient 
                        when descriptor is { IsKeyedService: true, KeyedImplementationFactory: not null }:
                        collection.Add(CreatePatchedKeyedFactoryDescriptor(descriptor));
                        break;
                    case ServiceLifetime.Transient 
                        when descriptor is { IsKeyedService: false, ImplementationFactory: not null }:
                        collection.Add(CreatePatchedFactoryDescriptor(descriptor));
                        break;
                    default:
                        collection.Add(descriptor);
                        break;
                }
            }

            collection.AddScoped<ThrowOnTransientDisposable>();

            return collection.BuildServiceProvider();
        }

        private static ServiceDescriptor CreatePatchedFactoryDescriptor(
            ServiceDescriptor original)
        {
            var newDescriptor = new ServiceDescriptor(
                original.ServiceType,
                (sp) =>
                {
                    var originalFactory = original.ImplementationFactory ?? 
                        throw new InvalidOperationException("originalFactory is null.");

                    var originalResult = originalFactory(sp);

                    var throwOnTransientDisposable = 
                        sp.GetRequiredService<ThrowOnTransientDisposable>();
                    if (throwOnTransientDisposable.ShouldThrow && 
                        originalResult is IDisposable d)
                    {
                        ThrowTransientDisposableException(d.GetType().Name);
                    }

                    return originalResult;
                },
                ServiceLifetime.Transient);

            return newDescriptor;
        }

        private static ServiceDescriptor CreatePatchedKeyedFactoryDescriptor(ServiceDescriptor original)
        {
            var newDescriptor = new ServiceDescriptor(
                original.ServiceType,
                original.ServiceKey,
                (sp, obj) =>
                {
                    var originalFactory = original.KeyedImplementationFactory ?? 
                                          throw new InvalidOperationException("KeyedImplementationFactory is null.");
        
                    var originalResult = originalFactory(sp, obj);
        
                    var throwOnTransientDisposable = sp.GetRequiredService<ThrowOnTransientDisposable>();
                    if (throwOnTransientDisposable.ShouldThrow && originalResult is IDisposable d)
                    {
                        ThrowTransientDisposableException(d.GetType().Name);
                    }

                    return originalResult;
                },
                ServiceLifetime.Transient);

            return newDescriptor;
        }

        private static ServiceDescriptor CreatePatchedDescriptor(
            ServiceDescriptor original)
        {
            var newDescriptor = new ServiceDescriptor(
                original.ServiceType,
                (sp) => {
                    var throwOnTransientDisposable = 
                        sp.GetRequiredService<ThrowOnTransientDisposable>();
                    if (throwOnTransientDisposable.ShouldThrow)
                    {
                        ThrowTransientDisposableException(original.ImplementationType?.Name);
                    }

                    if (original.ImplementationType is null)
                    {
                        throw new InvalidOperationException(
                            "ImplementationType is null.");
                    }

                    return ActivatorUtilities.CreateInstance(sp, 
                        original.ImplementationType);
                },
                ServiceLifetime.Transient);
    
            return newDescriptor;
        }
        
        private static void ThrowTransientDisposableException(string? typeName)
        {
            throw new InvalidOperationException(
                $"Trying to resolve transient disposable service {typeName} in the wrong scope. " +
                "Use an 'OwningComponentBase<T>' component base class for the service 'T' you are trying to resolve.");
        }
    }

    internal class ThrowOnTransientDisposable
    {
        public bool ShouldThrow { get; set; }
    }
}
