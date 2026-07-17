namespace EdiX.Generation;

/// <summary>
/// Configuration options for creating an EDI interchange.
/// </summary>
public sealed class EdiInterchangeOptions
{
    /// <summary>
    /// EDI dialect (X12 or EDIFACT).
    /// </summary>
    public required EdiDialect Dialect { get; init; }

    /// <summary>
    /// Delimiters to use in the interchange. If null, uses dialect defaults.
    /// </summary>
    public EdiDelimiters? Delimiters { get; init; }

    /// <summary>
    /// Control number options for generating envelope control numbers.
    /// </summary>
    public EdiControlNumberOptions? ControlNumberOptions { get; init; }

    // X12-specific properties
    /// <summary>
    /// X12 ISA05/06: Sender ID qualifier and ID.
    /// </summary>
    public string? SenderId { get; init; }

    /// <summary>
    /// X12 ISA07/08: Receiver ID qualifier and ID.
    /// </summary>
    public string? ReceiverId { get; init; }

    /// <summary>
    /// X12 ISA11: Repetition separator character.
    /// </summary>
    public char? RepetitionSeparator { get; init; }

    /// <summary>
    /// X12 ISA12: Version ID. Default: "00501".
    /// </summary>
    public string? VersionId { get; init; }

    /// <summary>
    /// X12 ISA14: Acknowledgment requested (0/1). Default: "0".
    /// </summary>
    public string? AcknowledgmentRequested { get; init; }

    /// <summary>
    /// X12 ISA15: Usage indicator (P=Production, T=Test). Default: "P".
    /// </summary>
    public string? UsageIndicator { get; init; }

    // EDIFACT-specific properties
    /// <summary>
    /// EDIFACT UNB: Syntax identifier (e.g., "UNOB:1").
    /// </summary>
    public string? SyntaxIdentifier { get; init; }

    /// <summary>
    /// EDIFACT UNB: Sender identification.
    /// </summary>
    public string? SenderIdentification { get; init; }

    /// <summary>
    /// EDIFACT UNB: Recipient identification.
    /// </summary>
    public string? RecipientIdentification { get; init; }

    /// <summary>
    /// EDIFACT UNB: Test indicator (1=test). Default: null (production).
    /// </summary>
    public string? TestIndicator { get; init; }
}