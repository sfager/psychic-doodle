namespace EdiX.Generation;

/// <summary>
/// Configuration for control number generation in EDI interchanges.
/// </summary>
public sealed class EdiControlNumberOptions
{
    /// <summary>
    /// Starting value for interchange control numbers. Default: 1.
    /// </summary>
    public int InterchangeControlStart { get; init; } = 1;

    /// <summary>
    /// Starting value for functional group control numbers. Default: 1.
    /// </summary>
    public int GroupControlStart { get; init; } = 1;

    /// <summary>
    /// Starting value for transaction set control numbers. Default: 1.
    /// </summary>
    public int TransactionControlStart { get; init; } = 1;

    /// <summary>
    /// Custom control number provider. If null, uses default sequential provider.
    /// </summary>
    public IControlNumberProvider? Provider { get; init; }
}