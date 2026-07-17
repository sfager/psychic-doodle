namespace EdiX.Schema;

/// <summary>
/// Specifies whether an element or segment is required.
/// </summary>
public enum EdiUsage
{
    /// <summary>
    /// The element or segment must be present.
    /// </summary>
    Mandatory,
    
    /// <summary>
    /// The element or segment may be present.
    /// </summary>
    Optional,
    
    /// <summary>
    /// The element or segment must not be present.
    /// </summary>
    NotUsed
}