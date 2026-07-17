namespace EdiX.Parsing;

/// <summary>
/// Specifies the severity of a parse error.
/// </summary>
public enum EdiParseErrorSeverity
{
    /// <summary>
    /// A fatal error that prevents successful parsing.
    /// </summary>
    Fatal,
    
    /// <summary>
    /// A non-fatal warning about potential issues.
    /// </summary>
    Warning
}