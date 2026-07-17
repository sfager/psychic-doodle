namespace EdiX.Parsing;

/// <summary>
/// Specifies the type of token read from an EDI stream.
/// </summary>
public enum EdiTokenType
{
    /// <summary>
    /// No token has been read yet.
    /// </summary>
    None,
    
    /// <summary>
    /// Start of an interchange (X12 ISA, EDIFACT UNB).
    /// </summary>
    InterchangeStart,
    
    /// <summary>
    /// End of an interchange (X12 IEA, EDIFACT UNZ).
    /// </summary>
    InterchangeEnd,
    
    /// <summary>
    /// Start of a functional group (X12 GS, EDIFACT UNG).
    /// </summary>
    FunctionalGroupStart,
    
    /// <summary>
    /// End of a functional group (X12 GE, EDIFACT UNE).
    /// </summary>
    FunctionalGroupEnd,
    
    /// <summary>
    /// Start of a transaction (X12 ST, EDIFACT UNH).
    /// </summary>
    TransactionStart,
    
    /// <summary>
    /// End of a transaction (X12 SE, EDIFACT UNT).
    /// </summary>
    TransactionEnd,
    
    /// <summary>
    /// A data segment.
    /// </summary>
    Segment
}