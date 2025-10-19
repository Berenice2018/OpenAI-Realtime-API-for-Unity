using System;
using System.Threading;

public static class CancellationUtils
{
    public static void SafeCancelAndDispose(ref CancellationTokenSource cts)
    {
        if (cts == null) return;

        try
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }
        catch (ObjectDisposedException) { }
        finally
        {
            cts.Dispose();
            cts = null;
        }
    }
}