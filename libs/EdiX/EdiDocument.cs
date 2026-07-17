using System.Collections.Immutable;
using System.Text;
using EdiX.Editing;
using EdiX.Parsing;
using EdiX.Parsing.Internal;

namespace EdiX;

/// <summary>
/// The root type of the EDI document object model. Immutable and thread-safe.
/// </summary>
public sealed class EdiDocument
{
    /// <summary>The interchange envelope that forms the root of the EDI tree.</summary>
    public EdiInterchange Interchange { get; }
    
    /// <summary>The delimiters used in this document.</summary>
    public EdiDelimiters Delimiters { get; }

    internal EdiDocument(EdiInterchange interchange, EdiDelimiters? delimiters = null)
    {
        ArgumentNullException.ThrowIfNull(interchange);
        Interchange = interchange;
        Delimiters = delimiters ?? (interchange.Dialect == EdiDialect.X12 
            ? EdiDelimiters.X12Defaults 
            : EdiDelimiters.EdifactDefaults);
    }

    // ── Surgical Editing ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns a new <see cref="EdiDocument"/> with the segment at <paramref name="address"/>
    /// modified by the supplied editor action. The original document is unchanged.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="address"/> refers to a non-existent location.</exception>
    public EdiDocument EditSegment(SegmentAddress address, Action<EdiSegmentEditor> edit)
    {
        ArgumentNullException.ThrowIfNull(edit);
        EdiSegment original = ResolveSegment(address);
        var editor = new EdiSegmentEditor(original);
        edit(editor);
        EdiSegment updated = editor.Build();
        return RebuildWithSegment(address, updated);
    }

    /// <summary>
    /// Returns a new <see cref="EdiDocument"/> with the X12 interchange header modified.
    /// The original document is unchanged.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the interchange dialect is not X12.</exception>
    public EdiDocument EditInterchangeHeader(Action<X12.X12InterchangeHeaderEditor> edit)
    {
        ArgumentNullException.ThrowIfNull(edit);
        if (Interchange.Dialect != EdiDialect.X12)
            throw new InvalidOperationException(
                "Cannot edit X12 interchange header: interchange dialect is EDIFACT.");
        var editor = new X12.X12InterchangeHeaderEditor(Interchange.HeaderSegment);
        edit(editor);
        return RebuildWithInterchangeHeader(editor.Build());
    }

    /// <summary>
    /// Returns a new <see cref="EdiDocument"/> with the EDIFACT interchange header modified.
    /// The original document is unchanged.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the interchange dialect is not EDIFACT.</exception>
    public EdiDocument EditInterchangeHeader(Action<Edifact.EdifactInterchangeHeaderEditor> edit)
    {
        ArgumentNullException.ThrowIfNull(edit);
        if (Interchange.Dialect != EdiDialect.Edifact)
            throw new InvalidOperationException(
                "Cannot edit EDIFACT interchange header: interchange dialect is X12.");
        var editor = new Edifact.EdifactInterchangeHeaderEditor(Interchange.HeaderSegment);
        edit(editor);
        return RebuildWithInterchangeHeader(editor.Build());
    }

    // ── Segment Operations (Phase 5) ──────────────────────────────────────────

    /// <summary>
    /// Returns a new <see cref="EdiDocument"/> with a segment inserted immediately before the addressed segment.
    /// The original document is unchanged.
    /// </summary>
    /// <param name="address">The address of the segment to insert before.</param>
    /// <param name="segment">The segment to insert.</param>
    /// <returns>A new document with the segment inserted.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="address"/> refers to a non-existent location.</exception>
    public EdiDocument InsertSegmentBefore(SegmentAddress address, EdiSegment segment) =>
        InsertSegment(address, segment, InsertPosition.Before);

    /// <summary>
    /// Returns a new <see cref="EdiDocument"/> with a segment inserted immediately after the addressed segment.
    /// The original document is unchanged.
    /// </summary>
    /// <param name="address">The address of the segment to insert after.</param>
    /// <param name="segment">The segment to insert.</param>
    /// <returns>A new document with the segment inserted.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="address"/> refers to a non-existent location.</exception>
    public EdiDocument InsertSegmentAfter(SegmentAddress address, EdiSegment segment) =>
        InsertSegment(address, segment, InsertPosition.After);

    private EdiDocument InsertSegment(SegmentAddress address, EdiSegment segment, InsertPosition position)
    {
        ArgumentNullException.ThrowIfNull(segment);
        
        // Validate the address and get the transaction
        var tx = ResolveTransaction(address);
        
        // Calculate insertion index
        int insertIndex = position == InsertPosition.Before
            ? address.SegmentPosition
            : address.SegmentPosition + 1;
        
        // Create new segments array with the inserted segment
        var newSegments = tx.Segments.Insert(insertIndex, segment);
        
        // Renumber all segments
        newSegments = RenumberSegments(newSegments);
        
        // Rebuild transaction with new segments
        var newTx = new EdiTransaction(
            tx.TransactionType,
            tx.HeaderSegment,
            tx.TrailerSegment,
            newSegments,
            tx.Loops,
            tx.SchemaKey
        );
        
        // Rebuild document
        return RebuildWithTransaction(address, newTx);
    }

    /// <summary>
    /// Returns a new <see cref="EdiDocument"/> with the segment at the specified address removed.
    /// The original document is unchanged.
    /// </summary>
    /// <param name="address">The address of the segment to remove.</param>
    /// <returns>A new document with the segment removed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="address"/> refers to a non-existent location.</exception>
    public EdiDocument RemoveSegment(SegmentAddress address)
    {
        // Validate the address exists
        var segment = ResolveSegment(address);
        var tx = ResolveTransaction(address);
        
        // Remove the segment at the position
        var newSegments = tx.Segments.RemoveAt(address.SegmentPosition);
        
        // Renumber remaining segments
        newSegments = RenumberSegments(newSegments);
        
        // Rebuild transaction with new segments
        var newTx = new EdiTransaction(
            tx.TransactionType,
            tx.HeaderSegment,
            tx.TrailerSegment,
            newSegments,
            tx.Loops,
            tx.SchemaKey
        );
        
        // Rebuild document
        return RebuildWithTransaction(address, newTx);
    }

    // InsertSegment and RemoveSegment are added in Phase 5.

    // ── Equality ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <see langword="true"/> when two documents have structurally equivalent content —
    /// same segments, same element values, in the same order.
    /// </summary>
    public static bool AreEquivalent(EdiDocument a, EdiDocument b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return InterchangesEquivalent(a.Interchange, b.Interchange);
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    private EdiSegment ResolveSegment(SegmentAddress address)
    {
        EdiTransaction tx = ResolveTransaction(address);
        if (address.SegmentPosition < 0 || address.SegmentPosition >= tx.Segments.Length)
            throw new ArgumentOutOfRangeException(nameof(address),
                $"Segment position {address.SegmentPosition} is out of range " +
                $"for transaction with {tx.Segments.Length} segment(s). Address: {address}");
        return tx.Segments[address.SegmentPosition];
    }

    private EdiTransaction ResolveTransaction(SegmentAddress address)
    {
        if (address.GroupIndex == -1)
        {
            if (address.TransactionIndex < 0 || address.TransactionIndex >= Interchange.Transactions.Length)
                throw new ArgumentOutOfRangeException(nameof(address),
                    $"Transaction index {address.TransactionIndex} is out of range. Address: {address}");
            return Interchange.Transactions[address.TransactionIndex];
        }

        if (address.GroupIndex < 0 || address.GroupIndex >= Interchange.Groups.Length)
            throw new ArgumentOutOfRangeException(nameof(address),
                $"Group index {address.GroupIndex} is out of range. Address: {address}");

        var group = Interchange.Groups[address.GroupIndex];
        if (address.TransactionIndex < 0 || address.TransactionIndex >= group.Transactions.Length)
            throw new ArgumentOutOfRangeException(nameof(address),
                $"Transaction index {address.TransactionIndex} is out of range " +
                $"for group with {group.Transactions.Length} transaction(s). Address: {address}");

        return group.Transactions[address.TransactionIndex];
    }

    private EdiDocument RebuildWithSegment(SegmentAddress address, EdiSegment newSegment)
    {
        if (address.GroupIndex == -1)
        {
            var tx = Interchange.Transactions[address.TransactionIndex];
            var newTx = RebuildTransaction(tx, address.SegmentPosition, newSegment);
            var newTransactions = Interchange.Transactions.SetItem(address.TransactionIndex, newTx);
            return new EdiDocument(new EdiInterchange(
                Interchange.Dialect, Interchange.Delimiters,
                Interchange.HeaderSegment, Interchange.TrailerSegment,
                Interchange.Groups, newTransactions));
        }

        var group = Interchange.Groups[address.GroupIndex];
        var txInGroup = group.Transactions[address.TransactionIndex];
        var newTxInGroup = RebuildTransaction(txInGroup, address.SegmentPosition, newSegment);
        var newGroupTransactions = group.Transactions.SetItem(address.TransactionIndex, newTxInGroup);
        var newGroup = new EdiFunctionalGroup(
            group.Dialect, group.HeaderSegment, group.TrailerSegment, newGroupTransactions);
        var newGroups = Interchange.Groups.SetItem(address.GroupIndex, newGroup);
        return new EdiDocument(new EdiInterchange(
            Interchange.Dialect, Interchange.Delimiters,
            Interchange.HeaderSegment, Interchange.TrailerSegment,
            newGroups, Interchange.Transactions));
    }

    private static EdiTransaction RebuildTransaction(
        EdiTransaction tx, int segmentPosition, EdiSegment newSegment) =>
        new(tx.TransactionType, tx.HeaderSegment, tx.TrailerSegment,
            tx.Segments.SetItem(segmentPosition, newSegment), tx.Loops, tx.SchemaKey);

    private EdiDocument RebuildWithTransaction(SegmentAddress address, EdiTransaction newTx)
    {
        if (address.GroupIndex == -1)
        {
            // EDIFACT ungrouped transaction
            var newTransactions = Interchange.Transactions.SetItem(address.TransactionIndex, newTx);
            return new EdiDocument(new EdiInterchange(
                Interchange.Dialect, Interchange.Delimiters,
                Interchange.HeaderSegment, Interchange.TrailerSegment,
                Interchange.Groups, newTransactions));
        }

        // X12 or EDIFACT grouped transaction
        var group = Interchange.Groups[address.GroupIndex];
        var newGroupTransactions = group.Transactions.SetItem(address.TransactionIndex, newTx);
        var newGroup = new EdiFunctionalGroup(
            group.Dialect, group.HeaderSegment, group.TrailerSegment, newGroupTransactions);
        var newGroups = Interchange.Groups.SetItem(address.GroupIndex, newGroup);
        return new EdiDocument(new EdiInterchange(
            Interchange.Dialect, Interchange.Delimiters,
            Interchange.HeaderSegment, Interchange.TrailerSegment,
            newGroups, Interchange.Transactions));
    }

    private static ImmutableArray<EdiSegment> RenumberSegments(ImmutableArray<EdiSegment> segments)
    {
        var renumbered = new EdiSegment[segments.Length];
        for (int i = 0; i < segments.Length; i++)
        {
            renumbered[i] = new EdiSegment(segments[i].Id, i, segments[i].Elements);
        }
        return renumbered.ToImmutableArray();
    }

    private EdiDocument RebuildWithInterchangeHeader(EdiSegment newHeader) =>
        new(new EdiInterchange(
            Interchange.Dialect, Interchange.Delimiters,
            newHeader, Interchange.TrailerSegment,
            Interchange.Groups, Interchange.Transactions));

    private static bool InterchangesEquivalent(EdiInterchange a, EdiInterchange b)
    {
        if (a.Dialect != b.Dialect) return false;
        if (!SegmentsEquivalent(a.HeaderSegment, b.HeaderSegment)) return false;
        if (!SegmentsEquivalent(a.TrailerSegment, b.TrailerSegment)) return false;
        if (a.Groups.Length != b.Groups.Length) return false;
        if (a.Transactions.Length != b.Transactions.Length) return false;
        for (int i = 0; i < a.Groups.Length; i++)
            if (!GroupsEquivalent(a.Groups[i], b.Groups[i])) return false;
        for (int i = 0; i < a.Transactions.Length; i++)
            if (!TransactionsEquivalent(a.Transactions[i], b.Transactions[i])) return false;
        return true;
    }

    private static bool GroupsEquivalent(EdiFunctionalGroup a, EdiFunctionalGroup b)
    {
        if (!SegmentsEquivalent(a.HeaderSegment, b.HeaderSegment)) return false;
        if (!SegmentsEquivalent(a.TrailerSegment, b.TrailerSegment)) return false;
        if (a.Transactions.Length != b.Transactions.Length) return false;
        for (int i = 0; i < a.Transactions.Length; i++)
            if (!TransactionsEquivalent(a.Transactions[i], b.Transactions[i])) return false;
        return true;
    }

    private static bool TransactionsEquivalent(EdiTransaction a, EdiTransaction b)
    {
        if (a.TransactionType != b.TransactionType) return false;
        if (!SegmentsEquivalent(a.HeaderSegment, b.HeaderSegment)) return false;
        if (!SegmentsEquivalent(a.TrailerSegment, b.TrailerSegment)) return false;
        if (a.Segments.Length != b.Segments.Length) return false;
        for (int i = 0; i < a.Segments.Length; i++)
            if (!SegmentsEquivalent(a.Segments[i], b.Segments[i])) return false;
        return true;
    }

    private static bool SegmentsEquivalent(EdiSegment a, EdiSegment b)
    {
        if (a.Id != b.Id) return false;
        if (a.Elements.Length != b.Elements.Length) return false;
        for (int i = 0; i < a.Elements.Length; i++)
            if (!ElementsEquivalent(a.Elements[i], b.Elements[i])) return false;
        return true;
    }

    private static bool ElementsEquivalent(EdiElement a, EdiElement b)
    {
        if (a.IsComposite != b.IsComposite) return false;
        if (!a.IsComposite) return a.Value == b.Value;
        if (a.Components.Length != b.Components.Length) return false;
        for (int i = 0; i < a.Components.Length; i++)
            if (a.Components[i].Value != b.Components[i].Value) return false;
        return true;
    }

    /// <inheritdoc/>
    public override string ToString() => $"EDI Document ({Interchange.Dialect})";

    // ── Parsing ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Parses an EDI document from a string.
    /// </summary>
    /// <param name="text">The EDI text.</param>
    /// <param name="options">Optional parsing options.</param>
    /// <returns>The parsed EDI document.</returns>
    /// <exception cref="Parsing.EdiParseException">Thrown when parsing fails in Strict mode.</exception>
    public static EdiDocument Parse(string text, EdiParseOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text, nameof(text));
        
        // Detect dialect
        var dialect = options?.DialectHint ?? DetectDialect(text);
        
        return dialect switch
        {
            EdiDialect.X12 => X12Parser.Parse(text, options),
            EdiDialect.Edifact => EdifactParser.Parse(text, options),
            _ => throw new EdiParseException(new EdiParseError(
                EdiParseErrorSeverity.Fatal,
                "PARSE-002",
                "Unable to detect EDI dialect",
                new EdiPosition { CharacterOffset = 0 }))
        };
    }

    /// <summary>
    /// Parses an EDI document from a stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="options">Optional parsing options.</param>
    /// <returns>The parsed EDI document.</returns>
    /// <exception cref="Parsing.EdiParseException">Thrown when parsing fails in Strict mode.</exception>
    public static EdiDocument Parse(Stream stream, EdiParseOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        options ??= new EdiParseOptions();
        
        using var reader = new StreamReader(stream, options.Encoding, leaveOpen: true);
        var text = reader.ReadToEnd();
        return Parse(text, options);
    }

    /// <summary>
    /// Attempts to parse an EDI document, returning errors instead of throwing.
    /// </summary>
    /// <param name="text">The EDI text.</param>
    /// <param name="options">Optional parsing options.</param>
    /// <returns>The parse result containing the document and any errors.</returns>
    public static EdiParseResult TryParse(string text, EdiParseOptions? options = null)
    {
        try
        {
            var document = Parse(text, options);
            return new EdiParseResult(document, Array.Empty<EdiParseError>());
        }
        catch (EdiParseException ex)
        {
            // Detect dialect for empty document
            var dialect = TryDetectDialect(text);
            var delimiters = dialect == EdiDialect.X12
                ? EdiDelimiters.X12Defaults
                : EdiDelimiters.EdifactDefaults;
            
            // Return a minimal document with the error
            var emptyDoc = new EdiDocument(new EdiInterchange(
                dialect,
                delimiters,
                new EdiSegment(dialect == EdiDialect.X12 ? "ISA" : "UNB", 0, ImmutableArray<EdiElement>.Empty),
                new EdiSegment(dialect == EdiDialect.X12 ? "IEA" : "UNZ", 0, ImmutableArray<EdiElement>.Empty),
                ImmutableArray<EdiFunctionalGroup>.Empty,
                ImmutableArray<EdiTransaction>.Empty));
            
            return new EdiParseResult(emptyDoc, new[] { ex.Error });
        }
    }

    /// <summary>
    /// Attempts to parse an EDI document from a stream, returning errors instead of throwing.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="options">Optional parsing options.</param>
    /// <returns>The parse result containing the document and any errors.</returns>
    public static EdiParseResult TryParse(Stream stream, EdiParseOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        options ??= new EdiParseOptions();
        
        using var reader = new StreamReader(stream, options.Encoding, leaveOpen: true);
        var text = reader.ReadToEnd();
        return TryParse(text, options);
    }

    /// <summary>
    /// Asynchronously parses an EDI document from a stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="options">Optional parsing options.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The parsed EDI document.</returns>
    /// <exception cref="Parsing.EdiParseException">Thrown when parsing fails in Strict mode.</exception>
    public static async Task<EdiDocument> ParseAsync(
        Stream stream,
        EdiParseOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        options ??= new EdiParseOptions();
        
        using var reader = new StreamReader(stream, options.Encoding, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return Parse(text, options);
    }

    /// <summary>
    /// Asynchronously parses an EDI document from a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="options">Optional parsing options.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The parsed EDI document.</returns>
    /// <exception cref="Parsing.EdiParseException">Thrown when parsing fails in Strict mode.</exception>
    public static async Task<EdiDocument> ParseFileAsync(
        string path,
        EdiParseOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, nameof(path));
        
        using var stream = File.OpenRead(path);
        return await ParseAsync(stream, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously attempts to parse an EDI document from a stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="options">Optional parsing options.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The parse result containing the document and any errors.</returns>
    public static async Task<EdiParseResult> TryParseAsync(
        Stream stream,
        EdiParseOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        options ??= new EdiParseOptions();
        
        using var reader = new StreamReader(stream, options.Encoding, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return TryParse(text, options);
    }

    /// <summary>
    /// Asynchronously parses multiple EDI documents from a stream.
    /// </summary>
    /// <param name="stream">The input stream containing multiple interchanges.</param>
    /// <param name="options">Optional parsing options.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An async enumerable of parsed documents.</returns>
    public static async IAsyncEnumerable<EdiDocument> ParseManyAsync(
        Stream stream,
        EdiParseOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        options ??= new EdiParseOptions();
        
        using var reader = new StreamReader(stream, options.Encoding, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        
        // Split by ISA (X12) or UNB (EDIFACT) to find multiple interchanges
        // For now, just parse as single document
        yield return Parse(text, options);
    }

    // ========== Generation Methods (Phase 4) ==========

    /// <summary>
    /// Creates a builder for constructing EDI documents programmatically.
    /// </summary>
    /// <param name="dialect">The EDI dialect (X12 or EDIFACT).</param>
    /// <param name="options">Optional interchange configuration options.</param>
    /// <returns>A new interchange builder.</returns>
    public static Generation.EdiInterchangeBuilder CreateBuilder(EdiDialect dialect, 
        Generation.EdiInterchangeOptions? options = null)
    {
        options ??= new Generation.EdiInterchangeOptions { Dialect = dialect };
        return new Generation.EdiInterchangeBuilder(options);
    }

    /// <summary>
    /// Writes this document to a stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="options">Optional write options.</param>
    public void WriteTo(Stream stream, Generation.EdiWriteOptions? options = null)
    {
        WriteToAsync(stream, options).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Asynchronously writes this document to a stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="options">Optional write options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task WriteToAsync(Stream stream, Generation.EdiWriteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var writer = Generation.EdiWriter.Create(stream, options);
        
        var interchangeOptions = new Generation.EdiInterchangeOptions
        {
            Dialect = Interchange.Dialect,
            Delimiters = Delimiters
        };
        
        await writer.WriteInterchangeStartAsync(interchangeOptions);
        
        // Write interchange header
        await writer.WriteSegmentAsync(Interchange.HeaderSegment);
        
        // Write groups
        foreach (var group in Interchange.Groups)
        {
            await writer.WriteSegmentAsync(group.HeaderSegment);
            
            foreach (var transaction in group.Transactions)
            {
                await writer.WriteSegmentAsync(transaction.HeaderSegment);
                
                foreach (var segment in transaction.Segments)
                {
                    await writer.WriteSegmentAsync(segment);
                }
                
                await writer.WriteSegmentAsync(transaction.TrailerSegment);
            }
            
            await writer.WriteSegmentAsync(group.TrailerSegment);
        }
        
        // Write ungrouped transactions (EDIFACT)
        foreach (var transaction in Interchange.Transactions)
        {
            await writer.WriteSegmentAsync(transaction.HeaderSegment);
            
            foreach (var segment in transaction.Segments)
            {
                await writer.WriteSegmentAsync(segment);
            }
            
            await writer.WriteSegmentAsync(transaction.TrailerSegment);
        }
        
        // Write interchange trailer
        await writer.WriteSegmentAsync(Interchange.TrailerSegment);
    }

    /// <summary>
    /// Converts this document to an EDI string.
    /// </summary>
    /// <param name="options">Optional write options.</param>
    /// <returns>The EDI string representation.</returns>
    public string ToEdiString(Generation.EdiWriteOptions? options = null)
    {
        using var stream = new MemoryStream();
        WriteTo(stream, options);
        stream.Position = 0;
        using var reader = new StreamReader(stream, options?.Encoding ?? Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static EdiDialect DetectDialect(string text)
    {
        if (text.StartsWith("ISA"))
        {
            return EdiDialect.X12;
        }
        
        if (text.StartsWith("UNA") || text.StartsWith("UNB"))
        {
            return EdiDialect.Edifact;
        }
        
        throw new EdiParseException(new EdiParseError(
            EdiParseErrorSeverity.Fatal,
            "PARSE-002",
            "Unable to detect EDI dialect",
            new EdiPosition { CharacterOffset = 0 }));
    }

    private static EdiDialect TryDetectDialect(string text)
    {
        if (text.StartsWith("ISA"))
        {
            return EdiDialect.X12;
        }
        
        if (text.StartsWith("UNA") || text.StartsWith("UNB"))
        {
            return EdiDialect.Edifact;
        }
        
        return EdiDialect.X12;  // Default fallback
    }
}
