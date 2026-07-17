namespace EdiX.Validation;

/// <summary>
/// Represents the severity of a validation error.
/// </summary>
public enum EdiValidationSeverity
{
    /// <summary>Warning - does not prevent processing.</summary>
    Warning,
    
    /// <summary>Error - prevents processing.</summary>
    Error
}
