namespace EdiX.Validation;

/// <summary>
/// Represents the layers of EDI validation.
/// </summary>
[Flags]
public enum EdiValidationLayer
{
    /// <summary>No validation.</summary>
    None = 0,
    
    /// <summary>Syntactic validation - segment structure, delimiters, control numbers.</summary>
    Syntactic = 1,
    
    /// <summary>Structural validation - schema conformance, required segments.</summary>
    Structural = 2,
    
    /// <summary>Semantic validation - data types, business rules.</summary>
    Semantic = 4,
    
    /// <summary>All validation layers.</summary>
    All = Syntactic | Structural | Semantic
}
