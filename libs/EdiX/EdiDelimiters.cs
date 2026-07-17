namespace EdiX;

/// <summary>
/// Holds the delimiter characters used within an EDI interchange.
/// </summary>
/// <remarks>
/// For X12, delimiters are detected from the fixed-width ISA segment header.
/// For EDIFACT, delimiters are read from the optional UNA service string advice
/// segment, or fall back to ISO 9735 defaults when UNA is absent.
/// </remarks>
public readonly record struct EdiDelimiters
{
    /// <summary>Character used to terminate a segment. X12 default: <c>~</c>. EDIFACT default: <c>'</c>.</summary>
    public required char Segment { get; init; }

    /// <summary>Character used to separate data elements within a segment. X12 default: <c>*</c>. EDIFACT default: <c>+</c>.</summary>
    public required char Element { get; init; }

    /// <summary>Character used to separate components within a composite element. Default: <c>:</c>.</summary>
    public required char Component { get; init; }

    /// <summary>Character used to separate repetitions of a simple element. X12 only (ISA11).</summary>
    public char? Repetition { get; init; }

    /// <summary>Release (escape) character. EDIFACT only. Default: <c>?</c>.</summary>
    public char? ReleaseChar { get; init; }

    /// <summary>Decimal notation character. EDIFACT only. Default: <c>.</c>.</summary>
    public char? DecimalNotation { get; init; }

    /// <summary>Standard X12 delimiter defaults.</summary>
    public static readonly EdiDelimiters X12Defaults = new()
    {
        Segment    = '~',
        Element    = '*',
        Component  = ':',
        Repetition = '^'
    };

    /// <summary>Standard EDIFACT delimiter defaults per ISO 9735.</summary>
    public static readonly EdiDelimiters EdifactDefaults = new()
    {
        Segment         = '\'',
        Element         = '+',
        Component       = ':',
        ReleaseChar     = '?',
        DecimalNotation = '.'
    };
}