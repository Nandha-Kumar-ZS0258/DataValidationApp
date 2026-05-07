using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Prism.Pipeline.Validation.Extensions;
using Xunit;

namespace Prism.Pipeline.Validation.Tests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPipelineValidation_RegistersIPipelineValidationServiceAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddPipelineValidation();

        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IPipelineValidationService));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        descriptor.ImplementationType.Should().Be(typeof(PipelineValidationService));
    }

    [Fact]
    public void AddPipelineValidation_ReturnsTheSameServiceCollection()
    {
        var services = new ServiceCollection();
        var returned = services.AddPipelineValidation();

        returned.Should().BeSameAs(services);
    }

    [Fact]
    public void AddPipelineValidation_ServiceIsResolvable()
    {
        var services = new ServiceCollection();
        services.AddPipelineValidation();

        using var provider = services.BuildServiceProvider();
        var svc = provider.GetService<IPipelineValidationService>();

        svc.Should().NotBeNull();
        svc.Should().BeOfType<PipelineValidationService>();
    }

    [Fact]
    public void AddPipelineValidation_ReturnsSameSingletonInstanceEachTime()
    {
        var services = new ServiceCollection();
        services.AddPipelineValidation();

        using var provider = services.BuildServiceProvider();
        var svc1 = provider.GetRequiredService<IPipelineValidationService>();
        var svc2 = provider.GetRequiredService<IPipelineValidationService>();

        svc1.Should().BeSameAs(svc2, "singleton must return the same instance on repeated resolution");
    }
}
