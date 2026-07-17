using System.Collections.Immutable;
using System.Text.Json;
using EdiX.Schema.Serialization.Dto;

namespace EdiX.Schema.Serialization;

/// <summary>
/// Serializes and deserializes <see cref="EdiTransactionSchema"/> to and from JSON.
/// </summary>
public static class EdiSchemaSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes a schema to a JSON string.
    /// </summary>
    /// <param name="schema">The schema to serialize.</param>
    /// <returns>JSON string representation.</returns>
    public static string Serialize(EdiTransactionSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        EdiTransactionSchemaDto dto = ToDto(schema);
        return JsonSerializer.Serialize(dto, _options);
    }

    /// <summary>
    /// Deserializes a schema from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized schema.</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or missing required fields.</exception>
    public static EdiTransactionSchema Deserialize(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        EdiTransactionSchemaDto? dto = JsonSerializer.Deserialize<EdiTransactionSchemaDto>(json, _options);
        if (dto == null)
        {
            throw new JsonException("JSON deserialized to null — the input may be empty or 'null'.");
        }
        return FromDto(dto);
    }

    /// <summary>
    /// Asynchronously deserializes a schema from a stream.
    /// </summary>
    /// <param name="stream">The JSON stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized schema.</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or missing required fields.</exception>
    public static async Task<EdiTransactionSchema> DeserializeFromStreamAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        EdiTransactionSchemaDto? dto = await JsonSerializer
            .DeserializeAsync<EdiTransactionSchemaDto>(stream, _options, cancellationToken)
            .ConfigureAwait(false);
        if (dto == null)
        {
            throw new JsonException("JSON stream deserialized to null — the input may be empty or 'null'.");
        }
        return FromDto(dto);
    }

    private static EdiTransactionSchemaDto ToDto(EdiTransactionSchema schema)
    {
        return new EdiTransactionSchemaDto
        {
            Key = KeyToDto(schema.Key),
            Description = schema.Description,
            Loops = schema.Loops.Select(LoopToDto).ToList(),
            Segments = schema.Segments.Select(SegmentToDto).ToList()
        };
    }

    private static EdiSchemaKeyDto KeyToDto(EdiSchemaKey key)
    {
        return new EdiSchemaKeyDto
        {
            Dialect = key.Dialect.ToString(),
            TransactionType = key.TransactionType,
            Version = key.Version,
            Release = key.Release,
            ControllingAgency = key.ControllingAgency,
            AssociationCode = key.AssociationCode
        };
    }

    private static EdiLoopSchemaDto LoopToDto(EdiLoopSchema loop)
    {
        return new EdiLoopSchemaDto
        {
            LoopId = loop.LoopId,
            TriggerSegmentId = loop.TriggerSegmentId,
            MaxRepeat = loop.MaxRepeat,
            SegmentIds = loop.SegmentIds.ToList(),
            ChildLoops = loop.ChildLoops.Select(LoopToDto).ToList()
        };
    }

    private static EdiSegmentSchemaDto SegmentToDto(EdiSegmentSchema segment)
    {
        return new EdiSegmentSchemaDto
        {
            Id = segment.Id,
            Description = segment.Description,
            Usage = segment.Usage.ToString(),
            MaxRepeat = segment.MaxRepeat,
            Elements = segment.Elements.Select(ElementToDto).ToList()
        };
    }

    private static EdiElementSchemaDto ElementToDto(EdiElementSchema element)
    {
        return new EdiElementSchemaDto
        {
            Position = element.Position,
            Name = element.Name,
            Usage = element.Usage.ToString(),
            DataType = element.DataType.ToString(),
            MinLength = element.MinLength,
            MaxLength = element.MaxLength,
            AllowedValues = element.AllowedValues?.ToList(),
            Conditions = element.Conditions?.Select(ConditionToDto).ToList()
        };
    }

    private static EdiConditionRuleDto ConditionToDto(EdiConditionRule rule)
    {
        return new EdiConditionRuleDto
        {
            ConditionType = rule.Type.ToString(),
            Positions = rule.ElementPositions.ToList()
        };
    }

    private static EdiTransactionSchema FromDto(EdiTransactionSchemaDto dto)
    {
        return new EdiTransactionSchema(
            key: KeyFromDto(dto.Key),
            description: dto.Description,
            loops: dto.Loops.Select(LoopFromDto).ToImmutableArray(),
            segments: dto.Segments.Select(SegmentFromDto).ToImmutableArray());
    }

    private static EdiSchemaKey KeyFromDto(EdiSchemaKeyDto dto)
    {
        if (!Enum.TryParse<EdiDialect>(dto.Dialect, ignoreCase: true, out var dialect))
        {
            throw new JsonException($"Unknown dialect '{dto.Dialect}'. Expected 'X12' or 'Edifact'.");
        }

        return dialect == EdiDialect.X12
            ? EdiSchemaKey.ForX12(dto.TransactionType, dto.Version, dto.Release)
            : EdiSchemaKey.ForEdifact(
                dto.TransactionType,
                dto.Version,
                dto.Release,
                dto.ControllingAgency ?? "UN",
                dto.AssociationCode);
    }

    private static EdiLoopSchema LoopFromDto(EdiLoopSchemaDto dto)
    {
        return new EdiLoopSchema(
            loopId: dto.LoopId,
            triggerSegmentId: dto.TriggerSegmentId,
            maxRepeat: dto.MaxRepeat,
            segmentIds: dto.SegmentIds.ToImmutableArray(),
            childLoops: dto.ChildLoops.Select(LoopFromDto).ToImmutableArray());
    }

    private static EdiSegmentSchema SegmentFromDto(EdiSegmentSchemaDto dto)
    {
        return new EdiSegmentSchema(
            id: dto.Id,
            description: dto.Description,
            usage: ParseEnum<EdiUsage>(dto.Usage, "usage"),
            maxRepeat: dto.MaxRepeat,
            elements: dto.Elements.Select(ElementFromDto).ToImmutableArray());
    }

    private static EdiElementSchema ElementFromDto(EdiElementSchemaDto dto)
    {
        return new EdiElementSchema(
            position: dto.Position,
            name: dto.Name,
            usage: ParseEnum<EdiUsage>(dto.Usage, "usage"),
            dataType: ParseEnum<EdiElementType>(dto.DataType, "dataType"),
            minLength: dto.MinLength,
            maxLength: dto.MaxLength,
            allowedValues: dto.AllowedValues != null
                ? dto.AllowedValues.ToImmutableArray()
                : null,
            conditions: dto.Conditions != null
                ? dto.Conditions.Select(ConditionFromDto).ToImmutableArray()
                : null);
    }

    private static EdiConditionRule ConditionFromDto(EdiConditionRuleDto dto)
    {
        return new EdiConditionRule(
            type: ParseEnum<EdiConditionType>(dto.ConditionType, "conditionType"),
            elementPositions: dto.Positions.ToImmutableArray());
    }

    private static T ParseEnum<T>(string value, string fieldName) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(value, ignoreCase: true, out var result))
        {
            throw new JsonException(
                $"Unknown {fieldName} value '{value}' for type {typeof(T).Name}.");
        }
        return result;
    }
}
