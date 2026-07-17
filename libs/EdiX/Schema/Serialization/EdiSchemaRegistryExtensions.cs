namespace EdiX.Schema.Serialization;

/// <summary>
/// Extension methods for populating <see cref="EdiSchemaRegistry"/> from an <see cref="IEdiSchemaStore"/>.
/// </summary>
public static class EdiSchemaRegistryExtensions
{
    /// <summary>
    /// Loads all schemas from the store and returns a new registry containing them.
    /// </summary>
    /// <param name="registry">The registry to extend.</param>
    /// <param name="store">The schema store to load from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A new <see cref="EdiSchemaRegistry"/> that includes all schemas loaded from <paramref name="store"/>
    /// in addition to those already in <paramref name="registry"/>.
    /// </returns>
    public static async Task<EdiSchemaRegistry> WithStoreAsync(
        this EdiSchemaRegistry registry,
        IEdiSchemaStore store,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(store);

        IReadOnlyList<EdiTransactionSchema> schemas =
            await store.LoadAllAsync(cancellationToken).ConfigureAwait(false);

        return registry.WithSchemas(schemas);
    }
}
