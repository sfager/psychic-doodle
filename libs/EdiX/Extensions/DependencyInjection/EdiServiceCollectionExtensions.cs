using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using EdiX.Schema;
using EdiX.Schema.Serialization;
using EdiX.Schema.Storage.Azure;
using EdiX.Schema.Storage.FileSystem;
using EdiX.Validation;

namespace EdiX.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering EDI services with dependency injection.
/// </summary>
public static class EdiServiceCollectionExtensions
{
    /// <summary>
    /// Adds EDI services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEdi(
        this IServiceCollection services,
        Action<EdiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register options
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<EdiOptions>(_ => { });
        }

        // Register schema registry as singleton
        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<EdiOptions>>();
            return options.Value.SchemaRegistry;
        });

        return services;
    }

    /// <summary>
    /// Adds EDI validation services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEdiValidation(
        this IServiceCollection services,
        Action<EdiValidatorOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Ensure base EDI services are registered
        services.AddEdi();

        // Register validator options
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<EdiValidatorOptions>(_ => { });
        }

        // Register validator as singleton
        services.TryAddSingleton<EdiValidator>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<EdiValidatorOptions>>();
            return EdiValidator.Create(options.Value);
        });

        return services;
    }

    /// <summary>
    /// Adds a custom schema registry to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action to build the registry.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEdiSchemaRegistry(
        this IServiceCollection services,
        Action<EdiSchemaRegistry> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        // Start with default registry and apply configuration
        var registry = EdiSchemaRegistry.Default;
        configure(registry);

        services.TryAddSingleton(registry);

        return services;
    }

    /// <summary>
    /// Adds a filesystem-backed schema store and registers it as <see cref="IEdiSchemaStore"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for filesystem schema store options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEdiFileSystemSchemaStore(
        this IServiceCollection services,
        Action<FileSystemSchemaStoreOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new FileSystemSchemaStoreOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton<FileSystemSchemaStore>();
        services.TryAddSingleton<IEdiSchemaStore>(sp => sp.GetRequiredService<FileSystemSchemaStore>());

        return services;
    }

    /// <summary>
    /// Adds an Azure Blob Storage-backed schema store and registers it as <see cref="IEdiSchemaStore"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for Azure blob schema store options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEdiAzureBlobSchemaStore(
        this IServiceCollection services,
        Action<AzureBlobSchemaStoreOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new AzureBlobSchemaStoreOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton<AzureBlobSchemaStore>();
        services.TryAddSingleton<IEdiSchemaStore>(sp => sp.GetRequiredService<AzureBlobSchemaStore>());

        return services;
    }
}
