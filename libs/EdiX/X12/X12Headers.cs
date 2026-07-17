namespace EdiX.X12;

/// <summary>Typed accessor for the X12 ISA (Interchange Control Header) segment. Zero-allocation readonly struct.</summary>
public readonly struct X12InterchangeHeader
{
    private readonly EdiSegment _isa;
    internal X12InterchangeHeader(EdiSegment isa) => _isa = isa;

    /// <summary>ISA01 — Authorization Qualifier.</summary>
    public string? AuthorizationQualifier => _isa.ElementValue(1);
    /// <summary>ISA02 — Authorization Information.</summary>
    public string? AuthorizationInfo      => _isa.ElementValue(2);
    /// <summary>ISA03 — Security Qualifier.</summary>
    public string? SecurityQualifier      => _isa.ElementValue(3);
    /// <summary>ISA04 — Security Information.</summary>
    public string? SecurityInfo           => _isa.ElementValue(4);
    /// <summary>ISA05 — Sender Qualifier.</summary>
    public string? SenderQualifier        => _isa.ElementValue(5);
    /// <summary>ISA06 — Interchange Sender ID.</summary>
    public string? SenderId               => _isa.ElementValue(6);
    /// <summary>ISA07 — Receiver Qualifier.</summary>
    public string? ReceiverQualifier      => _isa.ElementValue(7);
    /// <summary>ISA08 — Interchange Receiver ID.</summary>
    public string? ReceiverId             => _isa.ElementValue(8);
    /// <summary>ISA09 — Interchange Date (YYMMDD raw).</summary>
    public string? RawDate                => _isa.ElementValue(9);
    /// <summary>ISA09 parsed to DateOnly. Null when absent or unparseable.</summary>
    public DateOnly? Date =>
        DateOnly.TryParseExact(RawDate, "yyMMdd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var d) ? d : null;
    /// <summary>ISA10 — Interchange Time (HHmm raw).</summary>
    public string? RawTime                => _isa.ElementValue(10);
    /// <summary>ISA10 parsed to TimeOnly. Null when absent or unparseable.</summary>
    public TimeOnly? Time =>
        TimeOnly.TryParseExact(RawTime, "HHmm",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var t) ? t : null;
    /// <summary>ISA11 — Repetition Separator.</summary>
    public string? RepetitionSeparator    => _isa.ElementValue(11);
    /// <summary>ISA12 — Interchange Control Version Number (e.g. <c>00501</c>).</summary>
    public string? VersionNumber          => _isa.ElementValue(12);
    /// <summary>ISA13 — Interchange Control Number.</summary>
    public string? ControlNumber          => _isa.ElementValue(13);
    /// <summary>ISA14 — Acknowledgment Requested.</summary>
    public string? AcknowledgmentRequested => _isa.ElementValue(14);
    /// <summary>ISA15 — Usage Indicator (<c>P</c> = production, <c>T</c> = test).</summary>
    public string? UsageIndicator         => _isa.ElementValue(15);
    /// <summary>ISA16 — Component Element Separator.</summary>
    public char? ComponentSeparator       => _isa.ElementValue(16) is { Length: > 0 } v ? v[0] : null;
}

/// <summary>Typed accessor for the X12 IEA (Interchange Control Trailer) segment.</summary>
public readonly struct X12InterchangeTrailer
{
    private readonly EdiSegment _iea;
    internal X12InterchangeTrailer(EdiSegment iea) => _iea = iea;
    /// <summary>IEA01 — Number of Included Functional Groups.</summary>
    public string? GroupCount    => _iea.ElementValue(1);
    /// <summary>IEA02 — Interchange Control Number.</summary>
    public string? ControlNumber => _iea.ElementValue(2);
}

/// <summary>Typed accessor for the X12 GS (Functional Group Header) segment.</summary>
public readonly struct X12GroupHeader
{
    private readonly EdiSegment _gs;
    internal X12GroupHeader(EdiSegment gs) => _gs = gs;
    /// <summary>GS01 — Functional Identifier Code.</summary>
    public string? FunctionalIdentifier => _gs.ElementValue(1);
    /// <summary>GS02 — Application Sender Code.</summary>
    public string? SenderId             => _gs.ElementValue(2);
    /// <summary>GS03 — Application Receiver Code.</summary>
    public string? ReceiverId           => _gs.ElementValue(3);
    /// <summary>GS04 — Date (CCYYMMDD raw).</summary>
    public string? RawDate              => _gs.ElementValue(4);
    /// <summary>GS04 parsed to DateOnly. Null when absent or unparseable.</summary>
    public DateOnly? Date =>
        DateOnly.TryParseExact(RawDate, "yyyyMMdd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var d) ? d : null;
    /// <summary>GS05 — Time (HHmm raw).</summary>
    public string? RawTime              => _gs.ElementValue(5);
    /// <summary>GS06 — Group Control Number.</summary>
    public string? ControlNumber        => _gs.ElementValue(6);
    /// <summary>GS07 — Responsible Agency Code.</summary>
    public string? ResponsibleAgency    => _gs.ElementValue(7);
    /// <summary>GS08 — Version/Release (e.g. <c>004010</c>).</summary>
    public string? VersionRelease       => _gs.ElementValue(8);
}

/// <summary>Typed accessor for the X12 GE (Functional Group Trailer) segment.</summary>
public readonly struct X12GroupTrailer
{
    private readonly EdiSegment _ge;
    internal X12GroupTrailer(EdiSegment ge) => _ge = ge;
    /// <summary>GE01 — Number of Transaction Sets Included.</summary>
    public string? TransactionCount => _ge.ElementValue(1);
    /// <summary>GE02 — Group Control Number.</summary>
    public string? ControlNumber    => _ge.ElementValue(2);
}

/// <summary>Typed accessor for the X12 ST (Transaction Set Header) segment.</summary>
public readonly struct X12TransactionHeader
{
    private readonly EdiSegment _st;
    internal X12TransactionHeader(EdiSegment st) => _st = st;
    /// <summary>ST01 — Transaction Set Identifier Code.</summary>
    public string? TransactionSetId          => _st.ElementValue(1);
    /// <summary>ST02 — Transaction Set Control Number.</summary>
    public string? ControlNumber             => _st.ElementValue(2);
    /// <summary>ST03 — Implementation Convention Reference.</summary>
    public string? ImplementationConvention  => _st.ElementValue(3);
}

/// <summary>Typed accessor for the X12 SE (Transaction Set Trailer) segment.</summary>
public readonly struct X12TransactionTrailer
{
    private readonly EdiSegment _se;
    internal X12TransactionTrailer(EdiSegment se) => _se = se;
    /// <summary>SE01 — Number of Included Segments.</summary>
    public string? SegmentCount  => _se.ElementValue(1);
    /// <summary>SE02 — Transaction Set Control Number.</summary>
    public string? ControlNumber => _se.ElementValue(2);
}

/// <summary>Production/test usage indicator values for X12 ISA15.</summary>
public enum X12UsageIndicator
{
    /// <summary>Production data (<c>P</c>).</summary>
    Production = 'P',
    /// <summary>Test data (<c>T</c>).</summary>
    Test = 'T'
}