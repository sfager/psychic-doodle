using System.Collections.Immutable;

namespace EdiX.Generation;

/// <summary>
/// Builder for constructing EDI functional groups with transaction sets.
/// </summary>
public sealed class EdiGroupBuilder
{
    private readonly string _functionalIdentifier;
    private readonly List<EdiTransactionBuilder> _transactionBuilders = new();

    internal EdiGroupBuilder(string functionalIdentifier)
    {
        _functionalIdentifier = functionalIdentifier;
    }

    /// <summary>
    /// Adds a transaction set using a builder configuration action.
    /// </summary>
    /// <param name="transactionType">The transaction type (e.g., "850" for X12 PO, "ORDERS" for EDIFACT).</param>
    /// <param name="configure">Action to configure the transaction builder.</param>
    /// <returns>This builder for chaining.</returns>
    public EdiGroupBuilder AddTransaction(string transactionType, Action<EdiTransactionBuilder> configure)
    {
        var txBuilder = new EdiTransactionBuilder(transactionType);
        configure(txBuilder);
        _transactionBuilders.Add(txBuilder);
        return this;
    }

    internal async Task<EdiFunctionalGroup> BuildAsync(
        EdiDialect dialect,
        string groupControlNumber,
        IControlNumberProvider controlNumberProvider,
        EdiInterchangeOptions options)
    {
        // Build all transactions
        var transactions = new List<EdiTransaction>();
        foreach (var txBuilder in _transactionBuilders)
        {
            var txControlNumber = await controlNumberProvider.NextTransactionControlNumberAsync();
            transactions.Add(txBuilder.Build(dialect, txControlNumber));
        }

        // Build GS/GE or UNG/UNE envelope
        EdiSegment header, trailer;
        
        if (dialect == EdiDialect.X12)
        {
            // GS*PO*SENDER*RECEIVER*20231015*1200*1*X*004010
            var now = DateTime.Now;
            header = new EdiSegmentBuilder()
                .AddElement(_functionalIdentifier)
                .AddElement(options.SenderId ?? "SENDER")
                .AddElement(options.ReceiverId ?? "RECEIVER")
                .AddElement(now.ToString("yyyyMMdd"))
                .AddElement(now.ToString("HHmm"))
                .AddElement(groupControlNumber)
                .AddElement("X")
                .AddElement(options.VersionId ?? "004010")
                .Build("GS", 0);
            
            // GE*1*1
            trailer = new EdiSegmentBuilder()
                .AddElement(transactions.Count.ToString())
                .AddElement(groupControlNumber)
                .Build("GE", 1);
        }
        else
        {
            // UNG+ORDERS+SENDER+RECEIVER+231015:1200+1+UN+D:96A
            var now = DateTime.Now;
            header = new EdiSegmentBuilder()
                .AddElement(_functionalIdentifier)
                .AddElement(options.SenderIdentification ?? "SENDER")
                .AddElement(options.RecipientIdentification ?? "RECEIVER")
                .AddCompositeElement(now.ToString("yyMMdd"), now.ToString("HHmm"))
                .AddElement(groupControlNumber)
                .AddElement("UN")
                .AddCompositeElement("D", "96A")
                .Build("UNG", 0);
            
            // UNE+1+1
            trailer = new EdiSegmentBuilder()
                .AddElement(transactions.Count.ToString())
                .AddElement(groupControlNumber)
                .Build("UNE", 1);
        }

        return new EdiFunctionalGroup(
            dialect,
            header,
            trailer,
            transactions.ToImmutableArray()
        );
    }
}