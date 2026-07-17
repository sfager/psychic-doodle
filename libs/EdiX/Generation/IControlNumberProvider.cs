namespace EdiX.Generation;

/// <summary>
/// Provides control numbers for interchange, group, and transaction envelopes.
/// </summary>
public interface IControlNumberProvider
{
    /// <summary>
    /// Gets the next interchange control number (ISA13/UNB020).
    /// </summary>
    ValueTask<string> NextInterchangeControlNumberAsync();

    /// <summary>
    /// Gets the next functional group control number (GS06/UNG05).
    /// </summary>
    ValueTask<string> NextGroupControlNumberAsync();

    /// <summary>
    /// Gets the next transaction set control number (ST02/UNH01).
    /// </summary>
    ValueTask<string> NextTransactionControlNumberAsync();
}