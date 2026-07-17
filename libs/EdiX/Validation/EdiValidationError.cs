namespace EdiX.Validation;

/// <summary>
/// Represents a validation error found in an EDI document.
/// </summary>
public sealed class EdiValidationError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EdiValidationError"/> class.
    /// </summary>
    public EdiValidationError(
        EdiValidationLayer layer,
        EdiValidationSeverity severity,
        string code,
        string message,
        EdiPosition position,
        SegmentAddress? address = null,
        string? segmentId = null,
        int? elementPosition = null,
        string? loopId = null)
    {
        Layer = layer;
        Severity = severity;
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Position = position;
        Address = address;
        SegmentId = segmentId;
        ElementPosition = elementPosition;
        LoopId = loopId;
    }

    /// <summary>Gets the validation layer that detected this error.</summary>
    public EdiValidationLayer Layer { get; }

    /// <summary>Gets the severity of this error.</summary>
    public EdiValidationSeverity Severity { get; }

    /// <summary>Gets the stable error code.</summary>
    public string Code { get; }

    /// <summary>Gets the human-readable error message.</summary>
    public string Message { get; }

    /// <summary>Gets the position in the document where the error occurred.</summary>
    public EdiPosition Position { get; }

    /// <summary>Gets the segment address, if applicable.</summary>
    public SegmentAddress? Address { get; }

    /// <summary>Gets the segment ID where the error occurred, if applicable.</summary>
    public string? SegmentId { get; }

    /// <summary>Gets the element position within the segment, if applicable.</summary>
    public int? ElementPosition { get; }

    /// <summary>Gets the loop ID where the error occurred, if applicable.</summary>
    public string? LoopId { get; }
}
