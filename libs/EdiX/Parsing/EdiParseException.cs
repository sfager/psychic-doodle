namespace EdiX.Parsing;

/// <summary>
/// Exception thrown when parsing fails in Strict mode.
/// </summary>
public class EdiParseException : Exception
{
    /// <summary>
    /// Gets the parse error that caused this exception.
    /// </summary>
    public EdiParseError Error { get; }

    /// <summary>
    /// Initializes a new parse exception.
    /// </summary>
    /// <param name="error">The parse error.</param>
    public EdiParseException(EdiParseError error)
        : base(error.Message)
    {
        Error = error;
    }
}