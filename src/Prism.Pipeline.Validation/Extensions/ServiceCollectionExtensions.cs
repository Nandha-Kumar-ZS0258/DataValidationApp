using Microsoft.Extensions.DependencyInjection;

namespace Prism.Pipeline.Validation.Extensions;

/// <summary>
/// DI registration helpers for the Validation library.
/// Call <see cref="AddPipelineValidation"/> alongside <c>AddAdaptorCore()</c> in any host.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the pipeline validation service (Gate 1, Gate 2, Gate 3 orchestration).
    /// </summary>
    public static IServiceCollection AddPipelineValidation(this IServiceCollection services)
    {
        services.AddSingleton<IPipelineValidationService, PipelineValidationService>();
        return services;
    }
}
