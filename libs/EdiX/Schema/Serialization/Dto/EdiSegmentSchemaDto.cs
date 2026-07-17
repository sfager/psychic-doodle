using System.Text.Json.Serialization;

namespace EdiX.Schema.Serialization.Dto;

/// <summary>
/// JSON DTO for EdiSegmentSchema.
/// </summary>
public sealed class EdiSegmentSchemaDto
{
    /// <summary>Segment identifier (e.g., "BEG", "REF").</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Human-readable description of the segment.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Usage requirement: "Mandatory", "Optional", or "NotUsed".</summary>
    [JsonPropertyName("usage")]
    public string Usage { get; set; } = "Optional";

    /// <summary>Maximum number of times this segment can repeat.</summary>
    [JsonPropertyName("maxRepeat")]
    public int MaxRepeat { get; set; } = 1;

    /// <summary>Element schemas for this segment.</summary>
    [JsonPropertyName("elements")]
    public List<EdiElementSchemaDto> Elements { get; set; } = new();
}
