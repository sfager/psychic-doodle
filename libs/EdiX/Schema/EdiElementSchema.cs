using System.Collections.Immutable;

namespace EdiX.Schema;

/// <summary>
/// Defines the structure and constraints of an element within a segment.
/// </summary>
public sealed class EdiElementSchema
{
    /// <summary>
    /// Gets the 1-based position of this element within its segment.
    /// </summary>
    public int Position { get; }
    
    /// <summary>
    /// Gets the name of this element.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets whether this element is mandatory, optional, or not used.
    /// </summary>
    public EdiUsage Usage { get; }
    
    /// <summary>
    /// Gets the data type of this element.
    /// </summary>
    public EdiElementType DataType { get; }
    
    /// <summary>
    /// Gets the minimum length for this element.
    /// </summary>
    public int MinLength { get; }
    
    /// <summary>
    /// Gets the maximum length for this element.
    /// </summary>
    public int MaxLength { get; }
    
    /// <summary>
    /// Gets the list of allowed values, if restricted. Null if any value is allowed.
    /// </summary>
    public ImmutableArray<string>? AllowedValues { get; }
    
    /// <summary>
    /// Gets the conditional rules that apply to this element.
    /// </summary>
    public ImmutableArray<EdiConditionRule>? Conditions { get; }

    /// <summary>
    /// Initializes a new element schema.
    /// </summary>
    /// <param name="position">The element position.</param>
    /// <param name="name">The element name.</param>
    /// <param name="usage">The usage requirement.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="minLength">The minimum length.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="allowedValues">Optional list of allowed values.</param>
    /// <param name="conditions">Optional conditional rules.</param>
    public EdiElementSchema(
        int position,
        string name,
        EdiUsage usage,
        EdiElementType dataType,
        int minLength,
        int maxLength,
        ImmutableArray<string>? allowedValues = null,
        ImmutableArray<EdiConditionRule>? conditions = null)
    {
        Position = position;
        Name = name;
        Usage = usage;
        DataType = dataType;
        MinLength = minLength;
        MaxLength = maxLength;
        AllowedValues = allowedValues;
        Conditions = conditions;
    }
}