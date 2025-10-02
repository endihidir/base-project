using TMPro;
using UnityBase.UI.Config.SO;
using UnityBase.Runtime.Factories;
using UnityEngine;

namespace UnityBase.UI.Dynamic
{
    public class CoinViewUI : DynamicMonoView
    {
        [SerializeField] private TextMeshProUGUI _coinTxt;
        [SerializeField] private Transform _coinIconT;
        [SerializeField] private MoveInOutViewConfigSO _moveInOutViewConfigSo;
        [SerializeField] private BounceViewConfigSO _bounceViewConfigSo;

        private IMoveInOutAnimation _moveInOutAnim;

        protected override void Initialize()
        {
            var coinModel = OwnerContext.ResolveModel<CoinModel>().Initialize();
            var coinView  = OwnerContext.ResolveView<CoinView>().Initialize(_coinIconT, _coinTxt);
            var coinCollectAnimation = OwnerContext.ResolveAnimation<CoinCollectAnimation>();
            OwnerContext.ResolvePresenter<CoinPresenter>().Initialize(coinModel, coinView, coinCollectAnimation);
            
            _moveInOutAnim = OwnerContext.ResolveAnimation<MoveInOutAnimation>()
                                          .Initialize(_rectTransform)
                                          .Configure(_moveInOutViewConfigSo);
        }

        public override void OpenView() => _moveInOutAnim?.MoveIn();
        public override void CloseView() => _moveInOutAnim?.MoveOut();
        public override void OpenViewInstantly() => _moveInOutAnim?.MoveInInstantly();
        public override void CloseViewInstantly() => _moveInOutAnim?.MoveOutInstantly();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _moveInOutAnim?.Dispose();
        }
    }
}
