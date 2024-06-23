using System;

namespace UnityBase.SceneManagement
{
    public class LoadingProgress : IProgress<float>
    {
        public event Action<float> Progressed;
        
        private const float RATIO = 1f;
        public void Report(float value)
        {
            Progressed?.Invoke(value / RATIO);
        }
    }
}