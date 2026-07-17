using Azure.Core;

namespace EdiX.Schema.Storage.Azure;

/// <summary>
/// Configuration options for <see cref="AzureBlobSchemaStore"/>.
/// </summary>
public sealed class AzureBlobSchemaStoreOptions
{
    /// <summary>
    /// Gets or sets the Azure Storage connection string.
    /// Mutually exclusive with <see cref="ServiceUri"/> + <see cref="Credential"/>.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the Azure Blob Storage service URI (e.g., <c>https://account.blob.core.windows.net</c>).
    /// Use with <see cref="Credential"/> for managed identity or other token-based authentication.
    /// Mutually exclusive with <see cref="ConnectionString"/>.
    /// </summary>
    public Uri? ServiceUri { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="TokenCredential"/> for authenticating to Azure Storage.
    /// Typically <c>new DefaultAzureCredential()</c> for managed identity.
    /// Used only when <see cref="ServiceUri"/> is set.
    /// </summary>
    public TokenCredential? Credential { get; set; }

    /// <summary>
    /// Gets or sets the name of the Azure Blob Storage container that holds schema files.
    /// </summary>
    public string ContainerName { get; set; } = "edi-schemas";

    /// <summary>
    /// Gets or sets the prefix prepended to all blob paths.
    /// Defaults to <c>"edi-schemas"</c>, so blobs are addressed as
    /// <c>{SchemaPrefix}/{dialect}/{releaseVersion}/{transactionType}.json</c>.
    /// Set to an empty string to skip the prefix.
    /// </summary>
    public string SchemaPrefix { get; set; } = "edi-schemas";
}
