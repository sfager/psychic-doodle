namespace EdiX.Schema.Serialization;

/// <summary>
/// Abstraction for loading EDI schemas from an external storage source.
/// </summary>
public interface IEdiSchemaStore
{
    /// <summary>
    /// Loads a single schema by its key.
    /// </summary>
    /// <param name="key">The schema key to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The schema if found; <see langword="null"/> if no file exists for the given key.
    /// </returns>
    Task<EdiTransactionSchema?> LoadAsync(
        EdiSchemaKey key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all schemas available in this store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All schemas that could be successfully loaded.</returns>
    Task<IReadOnlyList<EdiTransactionSchema>> LoadAllAsync(
        CancellationToken cancellationToken = default);
}
