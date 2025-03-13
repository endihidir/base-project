using System.Threading;

namespace UnityBase.Extensions
{
    public static class CancellationTokenExtentions
    {
        public static void Refresh(ref CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                }
            }
            else
            {
                cancellationTokenSource = new CancellationTokenSource();
            }
        }
    }
}