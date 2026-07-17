namespace EdiX.Parsing;

/// <summary>
/// Specifies how the parser should handle errors during parsing.
/// </summary>
public enum EdiParseMode
{
    /// <summary>
    /// Throws <see cref="EdiParseException"/> on the first error encountered.
    /// </summary>
    Strict,
    
    /// <summary>
    /// Collects non-fatal errors and returns a result with warnings.
    /// </summary>
    Lenient,
    
    /// <summary>
    /// Never throws exceptions; attempts to recover from all errors.
    /// </summary>
    Recovery
}