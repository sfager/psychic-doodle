using EdiX.Editing;

namespace EdiX.X12;

/// <summary>
/// Provides named surgical editing of X12 ISA interchange header fields.
/// Obtain via <see cref="EdiDocument.EditInterchangeHeader(Action{X12InterchangeHeaderEditor})"/>.
/// </summary>
public sealed class X12InterchangeHeaderEditor
{
    private readonly EdiSegmentEditor _editor;
    internal X12InterchangeHeaderEditor(EdiSegment isa) => _editor = new EdiSegmentEditor(isa);

    /// <summary>Sets ISA13 — padded left to 9 digits with '0'.</summary>
    public X12InterchangeHeaderEditor SetControlNumber(string number)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        _editor.SetElement(13, number.PadLeft(9, '0'));
        return this;
    }

    /// <summary>Sets ISA09 formatted as yyMMdd.</summary>
    public X12InterchangeHeaderEditor SetDate(DateOnly date)
    {
        _editor.SetElement(9, date.ToString("yyMMdd"));
        return this;
    }

    /// <summary>Sets ISA10 formatted as HHmm.</summary>
    public X12InterchangeHeaderEditor SetTime(TimeOnly time)
    {
        _editor.SetElement(10, time.ToString("HHmm"));
        return this;
    }

    /// <summary>Sets ISA06 padded right to 15 characters.</summary>
    public X12InterchangeHeaderEditor SetSenderId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _editor.SetElement(6, id.PadRight(15));
        return this;
    }

    /// <summary>Sets ISA08 padded right to 15 characters.</summary>
    public X12InterchangeHeaderEditor SetReceiverId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _editor.SetElement(8, id.PadRight(15));
        return this;
    }

    /// <summary>Sets ISA15.</summary>
    public X12InterchangeHeaderEditor SetUsageIndicator(X12UsageIndicator indicator)
    {
        _editor.SetElement(15, ((char)indicator).ToString());
        return this;
    }

    internal EdiSegment Build() => _editor.Build();
}