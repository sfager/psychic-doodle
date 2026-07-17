using System.Collections.Immutable;

namespace EdiX.Generation;

/// <summary>
/// Builder for constructing complete EDI interchanges with groups and transactions.
/// </summary>
public sealed class EdiInterchangeBuilder
{
    private readonly EdiInterchangeOptions _options;
    private readonly IControlNumberProvider _controlNumberProvider;
    private readonly List<EdiGroupBuilder> _groupBuilders = new();
    private readonly List<EdiTransactionBuilder> _ungroupedTransactionBuilders = new();

    internal EdiInterchangeBuilder(EdiInterchangeOptions options)
    {
        _options = options;
        _controlNumberProvider = options.ControlNumberOptions?.Provider 
            ?? new DefaultControlNumberProvider(options.ControlNumberOptions ?? new EdiControlNumberOptions());
    }

    /// <summary>
    /// Adds a functional group with transactions (X12 only).
    /// </summary>
    /// <param name="functionalIdentifier">The functional identifier (e.g., "PO" for Purchase Order).</param>
    /// <param name="configure">Action to configure the group builder.</param>
    /// <returns>This builder for chaining.</returns>
    public EdiInterchangeBuilder AddGroup(string functionalIdentifier, Action<EdiGroupBuilder> configure)
    {
        if (_options.Dialect == EdiDialect.Edifact && _ungroupedTransactionBuilders.Count > 0)
        {
            throw new InvalidOperationException("Cannot add groups after adding ungrouped transactions in EDIFACT.");
        }

        var groupBuilder = new EdiGroupBuilder(functionalIdentifier);
        configure(groupBuilder);
        _groupBuilders.Add(groupBuilder);
        return this;
    }

    /// <summary>
    /// Adds an ungrouped transaction (EDIFACT only, but also works for X12 convenience).
    /// </summary>
    /// <param name="messageType">The message type (e.g., "ORDERS" for EDIFACT, "850" for X12).</param>
    /// <param name="configure">Action to configure the transaction builder.</param>
    /// <returns>This builder for chaining.</returns>
    public EdiInterchangeBuilder AddTransaction(string messageType, Action<EdiTransactionBuilder> configure)
    {
        if (_options.Dialect == EdiDialect.X12 && _groupBuilders.Count > 0)
        {
            throw new InvalidOperationException("Cannot mix AddGroup and AddTransaction in X12. Use AddGroup().AddTransaction() instead.");
        }

        var txBuilder = new EdiTransactionBuilder(messageType);
        configure(txBuilder);
        _ungroupedTransactionBuilders.Add(txBuilder);
        return this;
    }

    /// <summary>
    /// Builds the complete EDI document with all envelope segments and control numbers.
    /// </summary>
    /// <returns>The constructed EDI document.</returns>
    public async Task<EdiDocument> BuildAsync()
    {
        var dialect = _options.Dialect;
        var delimiters = _options.Delimiters ?? (dialect == EdiDialect.X12 
            ? EdiDelimiters.X12Defaults 
            : EdiDelimiters.EdifactDefaults);

        var interchangeControlNumber = await _controlNumberProvider.NextInterchangeControlNumberAsync();

        // Build groups or ungrouped transactions
        var groups = new List<EdiFunctionalGroup>();
        var ungroupedTransactions = new List<EdiTransaction>();

        if (_groupBuilders.Count > 0)
        {
            foreach (var groupBuilder in _groupBuilders)
            {
                var groupControlNumber = await _controlNumberProvider.NextGroupControlNumberAsync();
                groups.Add(await groupBuilder.BuildAsync(dialect, groupControlNumber, _controlNumberProvider, _options));
            }
        }
        else if (_ungroupedTransactionBuilders.Count > 0)
        {
            foreach (var txBuilder in _ungroupedTransactionBuilders)
            {
                var txControlNumber = await _controlNumberProvider.NextTransactionControlNumberAsync();
                ungroupedTransactions.Add(txBuilder.Build(dialect, txControlNumber));
            }
        }

        // Build ISA/IEA or UNB/UNZ envelope
        EdiSegment header, trailer;
        
        if (dialect == EdiDialect.X12)
        {
            var now = DateTime.Now;
            // ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *231015*1200*^*00501*000000001*0*P*:~
            header = new EdiSegmentBuilder()
                .AddElement("00")
                .AddElement("          ")  // 10 spaces
                .AddElement("00")
                .AddElement("          ")  // 10 spaces
                .AddElement("ZZ")
                .AddElement((_options.SenderId ?? "SENDER").PadRight(15))
                .AddElement("ZZ")
                .AddElement((_options.ReceiverId ?? "RECEIVER").PadRight(15))
                .AddElement(now.ToString("yyMMdd"))
                .AddElement(now.ToString("HHmm"))
                .AddElement((_options.RepetitionSeparator ?? delimiters.Repetition ?? '^').ToString())
                .AddElement(_options.VersionId ?? "00501")
                .AddElement(interchangeControlNumber.PadLeft(9, '0'))
                .AddElement(_options.AcknowledgmentRequested ?? "0")
                .AddElement(_options.UsageIndicator ?? "P")
                .AddElement(delimiters.Component.ToString())
                .Build("ISA", 0);
            
            // IEA*1*000000001
            trailer = new EdiSegmentBuilder()
                .AddElement(groups.Count.ToString())
                .AddElement(interchangeControlNumber.PadLeft(9, '0'))
                .Build("IEA", 1);
        }
        else
        {
            // UNB+UNOB:1+SENDER:1+RECEIVER:1+231015:1200+1
            var now = DateTime.Now;
            var syntaxParts = (_options.SyntaxIdentifier ?? "UNOB:1").Split(':');
            var senderParts = ((_options.SenderIdentification ?? "SENDER") + ":1").Split(':');
            var receiverParts = ((_options.RecipientIdentification ?? "RECEIVER") + ":1").Split(':');
            
            header = new EdiSegmentBuilder()
                .AddCompositeElement(syntaxParts)
                .AddCompositeElement(senderParts)
                .AddCompositeElement(receiverParts)
                .AddCompositeElement(now.ToString("yyMMdd"), now.ToString("HHmm"))
                .AddElement(interchangeControlNumber)
                .Build("UNB", 0);
            
            if (!string.IsNullOrEmpty(_options.TestIndicator))
            {
                // Add test indicator element if specified
                var elements = header.Elements.ToList();
                elements.Add(new EdiElement(elements.Count + 1, _options.TestIndicator, ImmutableArray<EdiComponent>.Empty));
                header = new EdiSegment("UNB", 0, elements.ToImmutableArray());
            }
            
            // UNZ+1+1
            var messageCount = groups.Count > 0 ? groups.Sum(g => g.Transactions.Length) : ungroupedTransactions.Count;
            trailer = new EdiSegmentBuilder()
                .AddElement(messageCount.ToString())
                .AddElement(interchangeControlNumber)
                .Build("UNZ", 1);
        }

        var interchange = new EdiInterchange(
            dialect,
            delimiters,
            header,
            trailer,
            groups.ToImmutableArray(),
            ungroupedTransactions.ToImmutableArray()
        );

        return new EdiDocument(interchange);
    }

    /// <summary>
    /// Synchronous wrapper for BuildAsync that blocks on the result.
    /// </summary>
    /// <returns>The constructed EDI document.</returns>
    public EdiDocument Build()
    {
        return BuildAsync().GetAwaiter().GetResult();
    }
}