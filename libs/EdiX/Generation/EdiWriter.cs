namespace EdiX.Generation;

/// <summary>
/// Low-level writer for streaming EDI content to an output stream.
/// </summary>
public sealed class EdiWriter : IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly StreamWriter _writer;
    private readonly EdiWriteOptions _options;
    private EdiDelimiters? _delimiters;
    private EdiDialect? _dialect;
    private bool _disposed;

    private EdiWriter(Stream stream, EdiWriteOptions options)
    {
        _stream = stream;
        _options = options;
        _writer = new StreamWriter(stream, options.Encoding, options.BufferSize, leaveOpen: true);
    }

    /// <summary>
    /// Creates a new writer for the specified stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="options">Optional write options.</param>
    /// <returns>A new EDI writer.</returns>
    public static EdiWriter Create(Stream stream, EdiWriteOptions? options = null)
    {
        return new EdiWriter(stream, options ?? new EdiWriteOptions());
    }

    /// <summary>
    /// Writes the interchange start (ISA or UNB) segment.
    /// </summary>
    public async Task WriteInterchangeStartAsync(EdiInterchangeOptions options)
    {
        _dialect = options.Dialect;
        _delimiters = options.Delimiters ?? (_dialect == EdiDialect.X12 
            ? EdiDelimiters.X12Defaults 
            : EdiDelimiters.EdifactDefaults);

        // Write UNA for EDIFACT if requested
        if (_dialect == EdiDialect.Edifact && _options.WriteEdifactUna)
        {
            await _writer.WriteAsync("UNA");
            await _writer.WriteAsync(_delimiters.Value.Component);
            await _writer.WriteAsync(_delimiters.Value.Element);
            await _writer.WriteAsync(_delimiters.Value.DecimalNotation ?? '.');
            await _writer.WriteAsync(_delimiters.Value.ReleaseChar ?? '?');
            await _writer.WriteAsync(' ');  // Reserved
            await _writer.WriteAsync(_delimiters.Value.Segment);
        }
    }

    /// <summary>
    /// Writes a segment with the specified ID and element values.
    /// </summary>
    public async Task WriteSegmentAsync(string id, params string[] elements)
    {
        if (_delimiters == null)
            throw new InvalidOperationException("Must call WriteInterchangeStartAsync first.");

        await _writer.WriteAsync(id);
        
        foreach (var element in elements)
        {
            await _writer.WriteAsync(_delimiters.Value.Element);
            await _writer.WriteAsync(element ?? string.Empty);
        }
        
        await _writer.WriteAsync(_delimiters.Value.Segment);
        
        if (_options.LineBreakAfterSegment)
        {
            await _writer.WriteLineAsync();
        }
    }

    /// <summary>
    /// Writes a complete segment object.
    /// </summary>
    internal async Task WriteSegmentAsync(EdiSegment segment)
    {
        if (_delimiters == null)
            throw new InvalidOperationException("Must call WriteInterchangeStartAsync first.");

        await _writer.WriteAsync(segment.Id);
        
        foreach (var element in segment.Elements)
        {
            await _writer.WriteAsync(_delimiters.Value.Element);
            
            if (element.IsComposite)
            {
                for (int i = 0; i < element.Components.Length; i++)
                {
                    if (i > 0)
                        await _writer.WriteAsync(_delimiters.Value.Component);
                    await _writer.WriteAsync(element.Components[i].Value ?? string.Empty);
                }
            }
            else
            {
                await _writer.WriteAsync(element.Value ?? string.Empty);
            }
        }
        
        await _writer.WriteAsync(_delimiters.Value.Segment);
        
        if (_options.LineBreakAfterSegment)
        {
            await _writer.WriteLineAsync();
        }
    }

    /// <summary>
    /// Writes the group start (GS or UNG) segment.
    /// </summary>
    public Task WriteGroupStartAsync(string functionalIdentifier)
    {
        // No-op for now - segments are written directly
        return Task.CompletedTask;
    }

    /// <summary>
    /// Writes the transaction start (ST or UNH) segment.
    /// </summary>
    public Task WriteTransactionStartAsync(string transactionType)
    {
        // No-op for now - segments are written directly
        return Task.CompletedTask;
    }

    /// <summary>
    /// Writes the transaction end (SE or UNT) segment.
    /// </summary>
    public Task WriteTransactionEndAsync()
    {
        // No-op for now - segments are written directly
        return Task.CompletedTask;
    }

    /// <summary>
    /// Writes the group end (GE or UNE) segment.
    /// </summary>
    public Task WriteGroupEndAsync()
    {
        // No-op for now - segments are written directly
        return Task.CompletedTask;
    }

    /// <summary>
    /// Writes the interchange end (IEA or UNZ) segment.
    /// </summary>
    public Task WriteInterchangeEndAsync()
    {
        // No-op for now - segments are written directly
        return Task.CompletedTask;
    }

    /// <summary>
    /// Flushes the writer and disposes resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _writer.Flush();
            _writer.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// Asynchronously flushes the writer and disposes resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _writer.FlushAsync();
            await _writer.DisposeAsync();
            _disposed = true;
        }
    }
}