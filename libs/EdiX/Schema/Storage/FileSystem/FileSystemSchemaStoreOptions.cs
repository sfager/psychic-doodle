namespace EdiX.Schema.Storage.FileSystem;

/// <summary>
/// Configuration options for <see cref="FileSystemSchemaStore"/>.
/// </summary>
public sealed class FileSystemSchemaStoreOptions
{
    /// <summary>
    /// Gets or sets the root directory under which schema files are located.
    /// The store looks for files at: <c>{RootPath}/edi-schemas/{dialect}/{releaseVersion}/{transactionType}.json</c>
    /// </summary>
    /// <example>
    /// Setting <see cref="RootPath"/> to <c>/var/edi</c> will load X12 850 v004010 from
    /// <c>/var/edi/edi-schemas/X12/004010/850.json</c>.
    /// </example>
    public string RootPath { get; set; } = Directory.GetCurrentDirectory();
}
