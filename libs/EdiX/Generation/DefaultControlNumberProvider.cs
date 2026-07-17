namespace EdiX.Generation;

/// <summary>
/// Default control number provider using sequential integers.
/// </summary>
internal sealed class DefaultControlNumberProvider : IControlNumberProvider
{
    private int _interchangeCounter;
    private int _groupCounter;
    private int _transactionCounter;

    public DefaultControlNumberProvider(EdiControlNumberOptions options)
    {
        _interchangeCounter = options.InterchangeControlStart;
        _groupCounter = options.GroupControlStart;
        _transactionCounter = options.TransactionControlStart;
    }

    public ValueTask<string> NextInterchangeControlNumberAsync()
    {
        var num = Interlocked.Increment(ref _interchangeCounter);
        return ValueTask.FromResult(num.ToString());
    }

    public ValueTask<string> NextGroupControlNumberAsync()
    {
        var num = Interlocked.Increment(ref _groupCounter);
        return ValueTask.FromResult(num.ToString());
    }

    public ValueTask<string> NextTransactionControlNumberAsync()
    {
        var num = Interlocked.Increment(ref _transactionCounter);
        return ValueTask.FromResult(num.ToString());
    }
}