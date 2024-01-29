namespace Kestrel.Client;

public readonly struct ValueStartResult
{
    public required bool Successful { get; init; }

    public Exception Error { get; private init; }

    public static ValueStartResult SetError(Exception exception)
    {
        return new ValueStartResult
        {
            Successful = false,
            Error = exception
        };
    }
    
    public static ValueStartResult SetResult(bool result)
    {
        return new ValueStartResult
        {
            Successful = result
        };
    }
    
    
}