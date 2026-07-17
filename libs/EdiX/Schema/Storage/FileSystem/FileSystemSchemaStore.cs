using EdiX.Schema.Serialization;

namespace EdiX.Schema.Storage.FileSystem;

/// <summary>
/// An <see cref="IEdiSchemaStore"/> that loads schemas from JSON files on the local filesystem.
/// </summary>
/// <remarks>
/// Schema files are expected at:
/// <c>{RootPath}/edi-schemas/{dialect}/{releaseVersion}/{transactionType}.json</c>
/// </remarks>
public sealed class FileSystemSchemaStore : IEdiSchemaStore
{
    private readonly FileSystemSchemaStoreOptions _options;

    /// <summary>
    /// Initializes a new <see cref="FileSystemSchemaStore"/> with the specified options.
    /// </summary>
    /// <param name="options">Store options, including the root directory path.</param>
    public FileSystemSchemaStore(FileSystemSchemaStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc />
    public async Task<EdiTransactionSchema?> LoadAsync(
        EdiSchemaKey key,
        CancellationToken cancellationToken = default)
    {
        string relativePath = EdiSchemaPath.ForKey(key);
        string fullPath = Path.Combine(_options.RootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(fullPath))
        {
            return null;
        }

        await using FileStream stream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);
        return await EdiSchemaSerializer.DeserializeFromStreamAsync(stream, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EdiTransactionSchema>> LoadAllAsync(
        CancellationToken cancellationToken = default)
    {
        string searchRoot = Path.Combine(_options.RootPath, "edi-schemas");

        if (!Directory.Exists(searchRoot))
        {
            return Array.Empty<EdiTransactionSchema>();
        }

        string[] files = Directory.GetFiles(searchRoot, "*.json", SearchOption.AllDirectories);
        var schemas = new List<EdiTransactionSchema>(files.Length);

        foreach (string file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using FileStream stream = new(file, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true);

            EdiTransactionSchema schema = await EdiSchemaSerializer
                .DeserializeFromStreamAsync(stream, cancellationToken)
                .ConfigureAwait(false);

            schemas.Add(schema);
        }

        return schemas;
    }
}
