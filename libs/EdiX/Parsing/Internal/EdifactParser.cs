using System.Collections.Immutable;
using System.Text;
using EdiX.Schema;

namespace EdiX.Parsing.Internal;

/// <summary>
/// Parses EDIFACT EDI documents.
/// </summary>
internal static class EdifactParser
{
    public static EdiDocument Parse(string text, EdiParseOptions? options = null)
    {
        options ??= new EdiParseOptions();
        var errors = new List<EdiParseError>();
        
        // Detect delimiters - check for UNA
        EdiDelimiters delimiters;
        int startPosition = 0;
        
        if (text.Length >= 9 && text.StartsWith("UNA"))
        {
            // Parse UNA service string (9 bytes)
            // UNA:+.? '
            // Position 3: Component separator
            // Position 4: Element separator
            // Position 5: Decimal notation
            // Position 6: Release character
            // Position 7: Reserved (space)
            // Position 8: Segment terminator
            
            delimiters = new EdiDelimiters
            {
                Component = text[3],
                Element = text[4],
                DecimalNotation = text[5],
                ReleaseChar = text[6],
                Segment = text[8],
                Repetition = '*'  // EDIFACT doesn't use repetition separator in UNA
            };
            
            startPosition = 9;  // Skip UNA when parsing segments
        }
        else
        {
            delimiters = options.DelimiterOverride ?? EdiDelimiters.EdifactDefaults;
        }
        
        // Handle release characters in the text
        text = ProcessReleaseCharacters(text.Substring(startPosition), delimiters);
        
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

        // Parse UNB
        var unbSegment = ParseSegment(segments[0], delimiters, 0);
        if (unbSegment.Id != "UNB")
        {
            throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "EDI-UNB-001",
                "Document must start with UNB segment",
                new EdiPosition { CharacterOffset = 0, SegmentIndex = 0 }));
        }

        // Find UNZ
        var unzIndex = segments.FindLastIndex(s => s.StartsWith("UNZ"));
        if (unzIndex < 0)
        {
            throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "EDI-UNB-002",
                "No UNZ segment found",
                new EdiPosition { CharacterOffset = 0 }));
        }

        var unzSegment = ParseSegment(segments[unzIndex], delimiters, unzIndex);
        
        // Check if grouped or ungrouped (UNG present or UNH directly after UNB)
        int pos = 1;  // Start after UNB
        var groups = new List<EdiFunctionalGroup>();
        var transactions = new List<EdiTransaction>();
        
        if (pos < unzIndex)
        {
            var firstSegId = GetSegmentId(segments[pos], delimiters);
            
            if (firstSegId == "UNG")
            {
                // Grouped EDIFACT
                while (pos < unzIndex)
                {
                    var segId = GetSegmentId(segments[pos], delimiters);
                    
                    if (segId == "UNG")
                    {
                        var group = ParseGroup(segments, delimiters, ref pos, errors, options);
                        groups.Add(group);
                    }
                    else
                    {
                        pos++;
                    }
                }
            }
            else if (firstSegId == "UNH")
            {
                // Ungrouped EDIFACT - transactions directly in interchange
                while (pos < unzIndex)
                {
                    var segId = GetSegmentId(segments[pos], delimiters);
                    
                    if (segId == "UNH")
                    {
                        var transaction = ParseTransaction(segments, delimiters, ref pos, unzIndex, errors, options);
                        transactions.Add(transaction);
                    }
                    else
                    {
                        pos++;
                    }
                }
            }
        }

        // Build interchange
        var interchange = new EdiInterchange(
            EdiDialect.Edifact,
            delimiters,
            unbSegment,
            unzSegment,
            groups.ToImmutableArray(),
            transactions.ToImmutableArray());

        return new EdiDocument(interchange, delimiters);
    }

    private static EdiFunctionalGroup ParseGroup(
        List<string> segments,
        EdiDelimiters delimiters,
        ref int pos,
        List<EdiParseError> errors,
        EdiParseOptions options)
    {
        var ungSegment = ParseSegment(segments[pos], delimiters, pos);
        pos++;

        // Find UNE
        int uneIndex = -1;
        for (int i = pos; i < segments.Count; i++)
        {
            if (GetSegmentId(segments[i], delimiters) == "UNE")
            {
                uneIndex = i;
                break;
            }
        }

        if (uneIndex < 0)
        {
            throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "EDI-GROUP-001",
                "No UNE segment found for UNG",
                new EdiPosition { CharacterOffset = 0, SegmentIndex = pos }));
        }

        var uneSegment = ParseSegment(segments[uneIndex], delimiters, uneIndex);

        // Parse transactions
        var transactions = new List<EdiTransaction>();
        
        while (pos < uneIndex)
        {
            var segId = GetSegmentId(segments[pos], delimiters);
            
            if (segId == "UNH")
            {
                var transaction = ParseTransaction(segments, delimiters, ref pos, uneIndex, errors, options);
                transactions.Add(transaction);
            }
            else
            {
                pos++;
            }
        }

        pos = uneIndex + 1;  // Move past UNE

        return new EdiFunctionalGroup(
            EdiDialect.Edifact,
            ungSegment,
            uneSegment,
            transactions.ToImmutableArray());
    }

    private static EdiTransaction ParseTransaction(
        List<string> segments,
        EdiDelimiters delimiters,
        ref int pos,
        int groupEnd,
        List<EdiParseError> errors,
        EdiParseOptions options)
    {
        var unhSegment = ParseSegment(segments[pos], delimiters, pos);
        
        // Get message type from UNH02-1
        var messageIdentifier = unhSegment.Element(2);
        var transactionType = messageIdentifier.Component(1).Value ?? "";
        var messageVersion = messageIdentifier.Component(2).Value ?? "";  // UNH02-2: version
        var messageRelease = messageIdentifier.Component(3).Value ?? ""; // UNH02-3: release
        
        // Build version string: D:96A format (version:release)
        var version = string.IsNullOrEmpty(messageVersion) ? "" : $"{messageVersion}:{messageRelease}";
        
        pos++;

        // Find UNT
        int untIndex = -1;
        for (int i = pos; i < groupEnd; i++)
        {
            if (GetSegmentId(segments[i], delimiters) == "UNT")
            {
                untIndex = i;
                break;
            }
        }

        if (untIndex < 0)
        {
            throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "EDI-UNT-002",
                "No UNT segment found for UNH",
                new EdiPosition { CharacterOffset = 0, SegmentIndex = pos }));
        }

        var untSegment = ParseSegment(segments[untIndex], delimiters, untIndex);

        // Parse body segments
        var bodySegments = new List<EdiSegment>();
        while (pos < untIndex)
        {
            bodySegments.Add(ParseSegment(segments[pos], delimiters, pos));
            pos++;
        }

        pos = untIndex + 1;  // Move past UNT

        // Resolve schema and materialize loops
        EdiSchemaKey? schemaKey = null;
        EdiTransactionSchema? schema = null;
        EdiLoopCollection? loops = null;

        if (options.SchemaRegistry is EdiSchemaRegistry registry && !string.IsNullOrEmpty(version))
        {
            // EDIFACT version format from UNH02: D:96A → version=D, release=96A
            var versionParts = version.Split(':');
            if (versionParts.Length >= 2)
            {
                var versionCode = versionParts[0];
                var releaseCode = versionParts[1];
                
                // UNH02-4 is controlling agency (default UN if not present)
                var controllingAgency = messageIdentifier.Component(4).Value ?? "UN";
                
                schemaKey = EdiSchemaKey.ForEdifact(transactionType, versionCode, releaseCode, controllingAgency);
                schema = registry.GetSchema(schemaKey.Value);
                
                if (schema != null)
                {
                    loops = LoopMaterializer.Materialize(bodySegments.ToImmutableArray(), schema);
                }
            }
        }

        return new EdiTransaction(
            transactionType,
            unhSegment,
            untSegment,
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

    /// <summary>
    /// Processes release characters in EDIFACT text.
    /// Release character (typically ?) escapes the next character.
    /// For example, ?+ becomes literal +, ?? becomes literal ?
    /// </summary>
    private static string ProcessReleaseCharacters(string text, EdiDelimiters delimiters)
    {
        if (delimiters.ReleaseChar == null)
        {
            return text;
        }

        var release = delimiters.ReleaseChar.Value;
        var sb = new StringBuilder(text.Length);
        
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == release && i + 1 < text.Length)
            {
                // Skip the release character and add the next character literally
                i++;
                sb.Append(text[i]);
            }
            else
            {
                sb.Append(text[i]);
            }
        }
        
        return sb.ToString();
    }
}