using System.Collections.Immutable;

namespace EdiX.Schema;

/// <summary>
/// Represents the schema for a complete EDI transaction set or message.
/// </summary>
public sealed class EdiTransactionSchema
{
    /// <summary>
    /// Gets the unique schema key identifying this transaction schema.
    /// </summary>
    public EdiSchemaKey Key { get; }
    
    /// <summary>
    /// Gets the human-readable description of this transaction type.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Gets the hierarchical loop definitions for this transaction.
    /// </summary>
    public ImmutableArray<EdiLoopSchema> Loops { get; }
    
    /// <summary>
    /// Gets the flat segment definitions for this transaction.
    /// </summary>
    public ImmutableArray<EdiSegmentSchema> Segments { get; }

    /// <summary>
    /// Initializes a new transaction schema.
    /// </summary>
    /// <param name="key">The schema key.</param>
    /// <param name="description">The description.</param>
    /// <param name="loops">The loop definitions.</param>
    /// <param name="segments">The segment definitions.</param>
    public EdiTransactionSchema(
        EdiSchemaKey key,
        string description,
        ImmutableArray<EdiLoopSchema> loops,
        ImmutableArray<EdiSegmentSchema> segments)
    {
        Key = key;
        Description = description;
        Loops = loops;
        Segments = segments;
    }
}