using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TruStage.Adaptor.Validation.Extensions;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAdaptorValidation_RegistersIPipelineValidationServiceAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddAdaptorValidation();

        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IPipelineValidationService));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        descriptor.ImplementationType.Should().Be(typeof(PipelineValidationService));
    }

    [Fact]
    public void AddAdaptorValidation_ReturnsTheSameServiceCollection()
    {
        var services = new ServiceCollection();
        var returned = services.AddAdaptorValidation();

        returned.Should().BeSameAs(services);
    }

    [Fact]
    public void AddAdaptorValidation_ServiceIsResolvable()
    {
        var services = new ServiceCollection();
        services.AddAdaptorValidation();

        using var provider = services.BuildServiceProvider();
        var svc = provider.GetService<IPipelineValidationService>();

        svc.Should().NotBeNull();
        svc.Should().BeOfType<PipelineValidationService>();
    }

    [Fact]
    public void AddAdaptorValidation_ReturnsSameSingletonInstanceEachTime()
    {
        var services = new ServiceCollection();
        services.AddAdaptorValidation();

        using var provider = services.BuildServiceProvider();
        var svc1 = provider.GetRequiredService<IPipelineValidationService>();
        var svc2 = provider.GetRequiredService<IPipelineValidationService>();

        svc1.Should().BeSameAs(svc2, "singleton must return the same instance on repeated resolution");
    }
}
