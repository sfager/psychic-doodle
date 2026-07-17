using System.Text;

namespace EdiX.Generation;

/// <summary>
/// Configuration options for writing EDI documents to streams.
/// </summary>
public sealed class EdiWriteOptions
{
    /// <summary>
    /// Text encoding to use. Default: UTF-8.
    /// </summary>
    public Encoding Encoding { get; init; } = Encoding.UTF8;

    /// <summary>
    /// Whether to add a line break after each segment. Default: false.
    /// </summary>
    public bool LineBreakAfterSegment { get; init; } = false;

    /// <summary>
    /// Whether to pad X12 ISA elements to fixed widths. Default: true.
    /// </summary>
    public bool PadIsaElements { get; init; } = true;

    /// <summary>
    /// Whether to write EDIFACT UNA service string. Default: true.
    /// </summary>
    public bool WriteEdifactUna { get; init; } = true;

    /// <summary>
    /// Buffer size for stream writing. Default: 4096 bytes.
    /// </summary>
    public int BufferSize { get; init; } = 4096;
}