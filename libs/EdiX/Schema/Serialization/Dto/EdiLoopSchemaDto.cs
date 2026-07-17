using System.Text.Json.Serialization;

namespace EdiX.Schema.Serialization.Dto;

/// <summary>
/// JSON DTO for EdiLoopSchema.
/// </summary>
public sealed class EdiLoopSchemaDto
{
    /// <summary>Loop identifier (e.g., "N1", "PO1").</summary>
    [JsonPropertyName("loopId")]
    public string LoopId { get; set; } = string.Empty;

    /// <summary>Segment ID whose appearance triggers a new loop iteration.</summary>
    [JsonPropertyName("triggerSegmentId")]
    public string TriggerSegmentId { get; set; } = string.Empty;

    /// <summary>Maximum number of times this loop can repeat.</summary>
    [JsonPropertyName("maxRepeat")]
    public int MaxRepeat { get; set; } = 1;

    /// <summary>Segment IDs that may appear within this loop.</summary>
    [JsonPropertyName("segmentIds")]
    public List<string> SegmentIds { get; set; } = new();

    /// <summary>Child loops nested within this loop.</summary>
    [JsonPropertyName("childLoops")]
    public List<EdiLoopSchemaDto> ChildLoops { get; set; } = new();
}
