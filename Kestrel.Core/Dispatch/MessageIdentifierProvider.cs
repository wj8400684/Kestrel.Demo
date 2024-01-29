namespace Kestrel.Core;

public sealed class MessageIdentifierProvider
{
    private readonly object _syncRoot = new();

    private uint _value;

    public void Reset()
    {
        lock (_syncRoot)
        {
            _value = 0;
        }
    }

    public uint GetNextIdentifier()
    {
        lock (_syncRoot)
        {
            _value++;

            if (_value == 0)
                _value = 1;

            return _value;
        }
    }
}