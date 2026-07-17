using System.Text.Json.Serialization;

namespace EdiX.Schema.Serialization.Dto;

/// <summary>
/// JSON DTO for EdiTransactionSchema — the root object in each schema file.
/// </summary>
public sealed class EdiTransactionSchemaDto
{
    /// <summary>Schema key identifying this transaction type.</summary>
    [JsonPropertyName("key")]
    public EdiSchemaKeyDto Key { get; set; } = new();

    /// <summary>Human-readable description (e.g., "Purchase Order").</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Hierarchical loop definitions for this transaction.</summary>
    [JsonPropertyName("loops")]
    public List<EdiLoopSchemaDto> Loops { get; set; } = new();

    /// <summary>Flat segment definitions for this transaction.</summary>
    [JsonPropertyName("segments")]
    public List<EdiSegmentSchemaDto> Segments { get; set; } = new();
}
