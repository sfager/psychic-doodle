using System.Collections.Immutable;

namespace EdiX.Schema;

/// <summary>
/// Represents a conditional rule that applies to element presence or values.
/// </summary>
public sealed class EdiConditionRule
{
    /// <summary>
    /// Gets the type of conditional rule.
    /// </summary>
    public EdiConditionType Type { get; }
    
    /// <summary>
    /// Gets the element positions involved in this condition.
    /// </summary>
    public ImmutableArray<int> ElementPositions { get; }

    /// <summary>
    /// Initializes a new condition rule.
    /// </summary>
    /// <param name="type">The condition type.</param>
    /// <param name="elementPositions">The element positions.</param>
    public EdiConditionRule(EdiConditionType type, ImmutableArray<int> elementPositions)
    {
        Type = type;
        ElementPositions = elementPositions;
    }
}