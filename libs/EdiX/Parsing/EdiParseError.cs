namespace EdiX.Parsing;

/// <summary>
/// Represents an error or warning encountered during EDI parsing.
/// </summary>
public sealed class EdiParseError
{
    /// <summary>
    /// Gets the severity of this error.
    /// </summary>
    public EdiParseErrorSeverity Severity { get; }
    
    /// <summary>
    /// Gets the stable error code (e.g., "X12-ISA-001").
    /// </summary>
    public string Code { get; }
    
    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Gets the position in the EDI document where the error occurred.
    /// </summary>
    public EdiPosition Position { get; }
    
    /// <summary>
    /// Gets the segment ID related to this error, if applicable.
    /// </summary>
    public string? SegmentId { get; }

    /// <summary>
    /// Initializes a new parse error.
    /// </summary>
    /// <param name="severity">The error severity.</param>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="position">The position where the error occurred.</param>
    /// <param name="segmentId">Optional segment ID.</param>
    public EdiParseError(
        EdiParseErrorSeverity severity,
        string code,
        string message,
        EdiPosition position,
        string? segmentId = null)
    {
        Severity = severity;
        Code = code;
        Message = message;
        Position = position;
        SegmentId = segmentId;
    }
}