namespace EdiX.Schema;

/// <summary>
/// Defines strategies for resolving schema keys when exact matches are not found.
/// </summary>
public enum SchemaResolutionStrategy
{
    /// <summary>
    /// Only return schemas with exact key matches.
    /// </summary>
    ExactOnly,
    
    /// <summary>
    /// If exact match fails, find the latest release matching the transaction type, dialect, and version.
    /// </summary>
    LatestMatchingVersion,
    
    /// <summary>
    /// Match any version/release for the transaction type and dialect.
    /// </summary>
    AnyVersion
}