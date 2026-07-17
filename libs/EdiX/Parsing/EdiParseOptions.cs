using System.Text;

namespace EdiX.Parsing;

/// <summary>
/// Configuration options for EDI parsing.
/// </summary>
public sealed class EdiParseOptions
{
    /// <summary>
    /// Gets the parse mode. Default is <see cref="EdiParseMode.Lenient"/>.
    /// </summary>
    public EdiParseMode Mode { get; init; } = EdiParseMode.Lenient;
    
    /// <summary>
    /// Gets the schema registry for validation and loop materialization.
    /// </summary>
    public object? SchemaRegistry { get; init; }  // Will be EdiSchemaRegistry in integrated version
    
    /// <summary>
    /// Gets a hint about the expected EDI dialect.
    /// </summary>
    public EdiDialect? DialectHint { get; init; }
    
    /// <summary>
    /// Gets delimiter overrides (skips auto-detection).
    /// </summary>
    public EdiDelimiters? DelimiterOverride { get; init; }
    
    /// <summary>
    /// Gets the text encoding. Default is UTF-8.
    /// </summary>
    public Encoding Encoding { get; init; } = Encoding.UTF8;
    
    /// <summary>
    /// Gets the maximum interchange size in bytes. Default is 100MB.
    /// </summary>
    public long MaxInterchangeSizeBytes { get; init; } = 100 * 1024 * 1024;
    
    /// <summary>
    /// Gets the maximum transaction size in bytes. Default is 10MB.
    /// </summary>
    public long MaxTransactionSizeBytes { get; init; } = 10 * 1024 * 1024;
    
    /// <summary>
    /// Gets a value indicating whether to perform syntactic validation during parsing. Default is true.
    /// </summary>
    public bool ValidateSyntaxOnParse { get; init; } = true;
    
    /// <summary>
    /// Gets the validator for semantic validation.
    /// </summary>
    public object? Validator { get; init; }  // Will be EdiValidator in Phase 7
    
    /// <summary>
    /// Gets the callback invoked for each warning encountered.
    /// </summary>
    public Action<EdiParseError>? OnWarning { get; init; }
}