using System.Collections.Immutable;

namespace EdiX.Parsing.Internal;

/// <summary>
/// Parses X12 EDI documents.
/// </summary>
internal static class X12Parser
{
    public static EdiDocument Parse(string text, EdiParseOptions? options = null)
    {
        options ??= new EdiParseOptions();
        var errors = new List<EdiParseError>();
        
        // Detect delimiters from ISA
        if (text.Length < 106)
        {
            throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "X12-ISA-002",
                "ISA segment must be at least 106 characters",
                new EdiPosition { CharacterOffset = 0, SegmentIndex = 0 }));
        }

        var isaBytes = options.Encoding.GetBytes(text.Substring(0, 106));
        var delimiters = options.DelimiterOverride ?? DelimiterDetector.DetectX12(isaBytes);
        
        // Split into segments
        var segments = text.Split(delimiters.Segment)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        if (segments.Count == 0)
        {
            throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "PARSE-001",
                "No segments found in document",
                new EdiPosition { CharacterOffset = 0 }));
        }

        // Parse ISA
        var isaSegment = ParseSegment(segments[0], delimiters, 0);
        if (isaSegment.Id != "ISA")
        {
            throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "X12-ISA-001",
                "Document must start with ISA segment",
                new EdiPosition { CharacterOffset = 0, SegmentIndex = 0 }));
        }

        // Find IEA
        var ieaIndex = segments.FindLastIndex(s => s.StartsWith("IEA"));
        if (ieaIndex < 0)
        {
            throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "X12-ISA-004",
                "No IEA segment found",
                new EdiPosition { CharacterOffset = 0 }));
        }

        var ieaSegment = ParseSegment(segments[ieaIndex], delimiters, ieaIndex);
        
        // Parse functional groups
        var groups = new List<EdiFunctionalGroup>();
        int pos = 1;  // Start after ISA
        
        while (pos < ieaIndex)
        {
            var segId = GetSegmentId(segments[pos], delimiters);
            
            if (segId == "GS")
            {
                var group = ParseGroup(segments, delimiters, ref pos, errors, options);
                groups.Add(group);
            }
            else
            {
                pos++;
            }
        }

        // Build interchange
        var interchange = new EdiInterchange(
            EdiDialect.X12,
            delimiters,
            isaSegment,
            ieaSegment,
            groups.ToImmutableArray(),
            ImmutableArray<EdiTransaction>.Empty);

        return new EdiDocument(interchange, delimiters);
    }

    private static EdiFunctionalGroup ParseGroup(
        List<string> segments,
        EdiDelimiters delimiters,
        ref int pos,
        List<EdiParseError> errors,
        EdiParseOptions options)
    {
        var gsSegment = ParseSegment(segments[pos], delimiters, pos);
        var gsVersion = gsSegment.Element(8).Value ?? "";  // GS08 is the version
        pos++;

        // Find GE
        int geIndex = -1;
        for (int i = pos; i < segments.Count; i++)
        {
            if (GetSegmentId(segments[i], delimiters) == "GE")
            {
                geIndex = i;
                break;
            }
        }

        if (geIndex < 0)
        {
            throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "X12-GS-002",
                "No GE segment found for GS",
                new EdiPosition { CharacterOffset = 0, SegmentIndex = pos }));
        }

        var geSegment = ParseSegment(segments[geIndex], delimiters, geIndex);

        // Parse transactions
        var transactions = new List<EdiTransaction>();
        
        while (pos < geIndex)
        {
            var segId = GetSegmentId(segments[pos], delimiters);
            
            if (segId == "ST")
            {
                var transaction = ParseTransaction(segments, delimiters, ref pos, geIndex, errors, options, gsVersion);
                transactions.Add(transaction);
            }
            else
            {
                pos++;
            }
        }

        pos = geIndex + 1;  // Move past GE

        return new EdiFunctionalGroup(
            EdiDialect.X12,
            gsSegment,
            geSegment,
            transactions.ToImmutableArray());
    }

    private static EdiTransaction ParseTransaction(
        List<string> segments,
        EdiDelimiters delimiters,
        ref int pos,
        int groupEnd,
        List<EdiParseError> errors,
        EdiParseOptions options,
        string? gsVersion = null)
    {
        var stSegment = ParseSegment(segments[pos], delimiters, pos);
        var transactionType = stSegment.Element(1).Value ?? "";
        pos++;

        // Find SE
        int seIndex = -1;
        for (int i = pos; i < groupEnd; i++)
        {
            if (GetSegmentId(segments[i], delimiters) == "SE")
            {
                seIndex = i;
                break;
            }
        }

        if (seIndex < 0)
        {
            throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "X12-ST-002",
                "No SE segment found for ST",
                new EdiPosition { CharacterOffset = 0, SegmentIndex = pos }));
        }

        var seSegment = ParseSegment(segments[seIndex], delimiters, seIndex);

        // Parse body segments
        var bodySegments = new List<EdiSegment>();
        while (pos < seIndex)
        {
            bodySegments.Add(ParseSegment(segments[pos], delimiters, pos));
            pos++;
        }

        pos = seIndex + 1;  // Move past SE

        // Resolve schema and materialize loops
        Schema.EdiSchemaKey? schemaKey = null;
        Schema.EdiTransactionSchema? schema = null;
        EdiLoopCollection? loops = null;

        if (options.SchemaRegistry is Schema.EdiSchemaRegistry registry && !string.IsNullOrEmpty(gsVersion) && gsVersion.Length >= 6)
        {
            // X12 version format: 004010 → version=004, release=010
            var version = gsVersion.Substring(0, 3);
            var release = gsVersion.Substring(3, 3);
            
            schemaKey = Schema.EdiSchemaKey.ForX12(transactionType, version, release);
            schema = registry.GetSchema(schemaKey.Value);
            
            if (schema != null)
            {
                loops = LoopMaterializer.Materialize(bodySegments.ToImmutableArray(), schema);
            }
        }

        return new EdiTransaction(
            transactionType,
            stSegment,
            seSegment,
            bodySegments.ToImmutableArray(),
            loops,
            schemaKey);
    }

    private static EdiSegment ParseSegment(string segmentText, EdiDelimiters delimiters, int position)
    {
        var elementStrings = segmentText.Split(delimiters.Element);
        var segmentId = elementStrings[0];
        
        var elements = new List<EdiElement>();
        for (int i = 1; i < elementStrings.Length; i++)
        {
            var elementText = elementStrings[i];
            
            if (elementText.Contains(delimiters.Component))
            {
                // Composite element
                var componentStrings = elementText.Split(delimiters.Component);
                var components = componentStrings
                    .Select((c, idx) => new EdiComponent(idx + 1, c))
                    .ToImmutableArray();
                elements.Add(EdiElement.Composite(i, components));
            }
            else
            {
                // Simple element
                elements.Add(EdiElement.Simple(i, elementText));
            }
        }

        return new EdiSegment(segmentId, position, elements.ToImmutableArray());
    }

    private static string GetSegmentId(string segmentText, EdiDelimiters delimiters)
    {
        var firstElement = segmentText.IndexOf(delimiters.Element);
        return firstElement > 0 
            ? segmentText.Substring(0, firstElement) 
            : segmentText;
    }
}