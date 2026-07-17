namespace EdiX.Schema;

/// <summary>
/// Specifies the data type of an element.
/// </summary>
public enum EdiElementType
{
    /// <summary>
    /// Alphanumeric string.
    /// </summary>
    AlphaNumeric,
    
    /// <summary>
    /// Date value.
    /// </summary>
    Date,
    
    /// <summary>
    /// Time value.
    /// </summary>
    Time,
    
    /// <summary>
    /// Integer numeric value.
    /// </summary>
    Numeric,
    
    /// <summary>
    /// Decimal numeric value with fractional part.
    /// </summary>
    Decimal,
    
    /// <summary>
    /// Identifier from a code list.
    /// </summary>
    Identifier
}