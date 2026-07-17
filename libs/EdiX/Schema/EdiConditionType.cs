namespace EdiX.Schema;

/// <summary>
/// Defines types of conditional relationships between elements.
/// </summary>
public enum EdiConditionType
{
    /// <summary>
    /// All listed elements must be present, or none may be present.
    /// </summary>
    AllOrNone,
    
    /// <summary>
    /// At least one of the listed elements must be present.
    /// </summary>
    AtLeastOne,
    
    /// <summary>
    /// Exactly one of the listed elements must be present.
    /// </summary>
    ExclusiveOr,
    
    /// <summary>
    /// If the first element is present, all others must be present.
    /// </summary>
    IfThenAll,
    
    /// <summary>
    /// One or more of the listed elements must be present.
    /// </summary>
    OneOrMore,
    
    /// <summary>
    /// Conditional based on a list of requirements.
    /// </summary>
    ListConditional
}