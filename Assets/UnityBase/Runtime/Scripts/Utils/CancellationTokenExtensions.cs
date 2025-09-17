using System.Threading;

namespace UnityBase.Extensions
{
    public static class CancellationTokenExtensions
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