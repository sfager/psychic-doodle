using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EdiX.Schema.Serialization;

namespace EdiX.Schema.Storage.Azure;

/// <summary>
/// An <see cref="IEdiSchemaStore"/> that loads schemas from JSON blobs in Azure Blob Storage.
/// </summary>
/// <remarks>
/// Blob paths follow the same convention as <see cref="EdiSchemaPath"/>:
/// <c>{SchemaPrefix}/{dialect}/{releaseVersion}/{transactionType}.json</c>
/// where the container is specified by <see cref="AzureBlobSchemaStoreOptions.ContainerName"/>.
/// </remarks>
public sealed class AzureBlobSchemaStore : IEdiSchemaStore
{
    private readonly AzureBlobSchemaStoreOptions _options;
    private readonly BlobContainerClient _containerClient;

    /// <summary>
    /// Initializes a new <see cref="AzureBlobSchemaStore"/> with the specified options.
    /// </summary>
    /// <param name="options">Store options, including connection details and container name.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when neither <see cref="AzureBlobSchemaStoreOptions.ConnectionString"/> nor
    /// <see cref="AzureBlobSchemaStoreOptions.ServiceUri"/> is configured.
    /// </exception>
    public AzureBlobSchemaStore(AzureBlobSchemaStoreOptions options)
        : this(options, containerClient: null)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="AzureBlobSchemaStore"/> with a pre-constructed container client.
    /// Intended for testing.
    /// </summary>
    /// <param name="options">Store options.</param>
    /// <param name="containerClient">A pre-constructed <see cref="BlobContainerClient"/>.</param>
    internal AzureBlobSchemaStore(AzureBlobSchemaStoreOptions options, BlobContainerClient? containerClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _containerClient = containerClient ?? CreateContainerClient(options);
    }

    /// <inheritdoc />
    public async Task<EdiTransactionSchema?> LoadAsync(
        EdiSchemaKey key,
        CancellationToken cancellationToken = default)
    {
        string blobPath = BuildBlobPath(EdiSchemaPath.ForKey(key));
        BlobClient blobClient = _containerClient.GetBlobClient(blobPath);

        try
        {
            Response<BlobDownloadStreamingResult> response =
                await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            await using Stream stream = response.Value.Content;
            return await EdiSchemaSerializer.DeserializeFromStreamAsync(stream, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EdiTransactionSchema>> LoadAllAsync(
        CancellationToken cancellationToken = default)
    {
        string prefix = string.IsNullOrEmpty(_options.SchemaPrefix)
            ? string.Empty
            : _options.SchemaPrefix.TrimEnd('/') + "/";

        var schemas = new List<EdiTransactionSchema>();

        await foreach (BlobItem blobItem in _containerClient
            .GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken)
            .ConfigureAwait(false))
        {
            if (!blobItem.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            BlobClient blobClient = _containerClient.GetBlobClient(blobItem.Name);
            Response<BlobDownloadStreamingResult> response =
                await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            await using Stream stream = response.Value.Content;
            EdiTransactionSchema schema = await EdiSchemaSerializer
                .DeserializeFromStreamAsync(stream, cancellationToken)
                .ConfigureAwait(false);

            schemas.Add(schema);
        }

        return schemas;
    }

    private string BuildBlobPath(string relativePath)
    {
        if (string.IsNullOrEmpty(_options.SchemaPrefix))
        {
            // relativePath is already "edi-schemas/X12/004010/850.json";
            // strip the leading "edi-schemas/" folder since it's the container itself
            return relativePath;
        }

        // Replace the "edi-schemas" folder in the relative path with the configured prefix
        // to support custom prefix structures.
        string withoutRoot = relativePath.StartsWith("edi-schemas/", StringComparison.Ordinal)
            ? relativePath["edi-schemas/".Length..]
            : relativePath;

        return _options.SchemaPrefix.TrimEnd('/') + "/" + withoutRoot;
    }

    private static BlobContainerClient CreateContainerClient(AzureBlobSchemaStoreOptions options)
    {
        if (!string.IsNullOrEmpty(options.ConnectionString))
        {
            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        }

        if (options.ServiceUri != null && options.Credential != null)
        {
            var serviceClient = new BlobServiceClient(options.ServiceUri, options.Credential);
            return serviceClient.GetBlobContainerClient(options.ContainerName);
        }

        throw new ArgumentException(
            $"Either {nameof(AzureBlobSchemaStoreOptions.ConnectionString)} or both " +
            $"{nameof(AzureBlobSchemaStoreOptions.ServiceUri)} and " +
            $"{nameof(AzureBlobSchemaStoreOptions.Credential)} must be configured.",
            nameof(options));
    }
}
