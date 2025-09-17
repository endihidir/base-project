using UnityEngine;

namespace UnityBase.Runtime.Behaviours
{
    public class CoinController : ICoinController
    {
        private ICoinModel _model;
        private ICoinView _view;
        private ICoinCollectAnimation _animation;
        
        public ICoinController Initialize(IOwnerContext ctx)
        {
            ctx.TryGetModel(out _model);
            ctx.TryGetView(out _view);
            ctx.TryGetAnimation(out _animation);
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

    public interface ICoinController : IController
    {
        public ICoinController Initialize(IOwnerContext ctx);
        void PlayCollect(int amount, CoinIconTest prefab, Transform parent, Vector3 startScreenPos);
    }
}