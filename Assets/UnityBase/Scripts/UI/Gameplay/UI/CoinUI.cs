using TMPro;
using UnityBase.UI.Config.SO;
using UnityBase.UI.ViewCore;
using UnityEngine;

namespace UnityBase.UI.Dynamic
{
    public class CoinUI : DynamicUI
    {
        [SerializeField] private TextMeshProUGUI _coinTxt;

        [SerializeField] private Transform _coinIconT;

        [SerializeField] private MoveInOutViewConfigSO _moveInOutViewConfigSo;

        [SerializeField] private BounceViewConfigSO _bounceViewConfigSo;

        private IBounceView _bounceCoinView;
        private IMoveInOutView _moveInOutAnim;
        private ICoinModel _coinModel;

        protected override void Initialize(IViewBehaviourFactory viewBehaviourFactory)
        {
            _coinModel = viewBehaviourFactory.CreateViewModel<CoinModel>(this)
                .Initialize(_coinTxt);

            _bounceCoinView = viewBehaviourFactory.CreateViewAnimation<BounceView>(this)
                .Initialize(_coinIconT)
                .Configure(_bounceViewConfigSo);
            
            _moveInOutAnim = viewBehaviourFactory.CreateViewLocalAnimation<MoveInOutView>()
                .Initialize(_rectTransform)
                .Configure(_moveInOutViewConfigSo);
        }

        public override void OpenView()
        {
            _moveInOutAnim?.MoveIn();
        }

        public override void CloseView()
        {
            _moveInOutAnim?.MoveOut();
        }

        public override void OpenViewInstantly()
        {
            _moveInOutAnim?.MoveInInstantly();
        }

        public override void CloseViewInstantly()
        {
            _moveInOutAnim?.MoveOutInstantly();
        }

        protected override void OnDestroy()
        {
            _moveInOutAnim?.Dispose();
            _coinModel?.Dispose();
            _bounceCoinView?.Dispose();
        }
    }
}