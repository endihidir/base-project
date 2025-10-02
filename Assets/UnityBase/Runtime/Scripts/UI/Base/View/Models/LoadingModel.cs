using UnityEngine;

namespace UnityBase.Runtime.Factories
{
    public class LoadingModel : ILoadingModel
    {
        public float FillAmount { get; private set; }
        public float TargetRatio { get; private set; }

        private float _fillSpeed;

        public ILoadingModel Initialize(float fillSpeed)
        {
            _fillSpeed = fillSpeed;
            return this;
        }

        public void SetTargetRatio(float val)
        {
            val = Mathf.Clamp01(val);
            TargetRatio = Mathf.Max(TargetRatio, val);
        }

        public void UpdateData()
        {
            var diff = Mathf.Abs(TargetRatio - FillAmount);
            if (diff <= 0.001f)
            {
                FillAmount = TargetRatio;
                return;
            }

            var t = Mathf.Clamp01(Time.deltaTime * _fillSpeed * diff);
            FillAmount = Mathf.MoveTowards(FillAmount, TargetRatio, t);
            FillAmount = Mathf.Clamp01(FillAmount);
        }

        public void ResetProgress()
        {
            TargetRatio = 0f;
            FillAmount = 0f;
        }

        public void Dispose() { }
    }

    public interface ILoadingModel : IModel
    {
        float FillAmount { get; }
        float TargetRatio { get; }
        ILoadingModel Initialize(float fillSpeed);
        void SetTargetRatio(float val);
        void UpdateData();
        void ResetProgress();
    }
}