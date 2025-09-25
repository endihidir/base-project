using UnityEngine;

namespace UnityBase.Runtime.Factories
{
    public class CoinPresenter : ICoinPresenter
    {
        private ICoinModel _model;
        private ICoinView _view;
        private ICoinCollectAnimation _animation;
        
        public ICoinPresenter Initialize(ICoinModel model, ICoinView view, ICoinCollectAnimation animation)
        {
            _model = model;
            _view = view;
            _animation = animation;
            _view.UpdateView(_model.Coins.Value);
            return this;
        }

        public void PlayCollect(int amount, CoinIconTest prefab, Transform parent, Vector3 startScreenPos)
        {
            _model.Add(amount);
            _animation.Configure(prefab, parent, startScreenPos, _view.CoinIconTransform);
            _animation.Play(UpdateView);
        }
        
        private void UpdateView() => _view.UpdateView(_model.Coins.Value);

        public void Dispose()
        {
            _view = null;
            _model = null;
            _animation = null;
        }
    }

    public interface ICoinPresenter : IPresenter
    {
        public ICoinPresenter Initialize(ICoinModel model, ICoinView view, ICoinCollectAnimation animation);
        void PlayCollect(int amount, CoinIconTest prefab, Transform parent, Vector3 startScreenPos);
    }
}