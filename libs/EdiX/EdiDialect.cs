namespace EdiX;

/// <summary>
/// Specifies the EDI dialect (standard) to use for parsing and validation.
/// </summary>
public enum EdiDialect
{
    /// <summary>ANSI X12 EDI standard.</summary>
    X12,
    
    /// <summary>UN/EDIFACT EDI standard (ISO 9735).</summary>
    Edifact
}