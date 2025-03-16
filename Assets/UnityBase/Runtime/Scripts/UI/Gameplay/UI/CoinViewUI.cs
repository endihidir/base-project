using TMPro;
using UnityBase.UI.Config.SO;
using UnityBase.UI.ViewCore;
using UnityEngine;

namespace UnityBase.UI.Dynamic
{
    public class CoinViewUI : DynamicUI
    {
        [SerializeField] private TextMeshProUGUI _coinTxt;

        [SerializeField] private Transform _coinIconT;

        [SerializeField] private MoveInOutViewConfigSO _moveInOutViewConfigSo;

        [SerializeField] private BounceViewConfigSO _bounceViewConfigSo;
        
        private IMoveInOutAnimation _moveInOutAnim;
        private ICoinModel _coinModel;
        private ICoinView _coinView;

        protected override void Initialize(IViewBehaviourFactory viewBehaviourFactory)
        {
            _coinModel = viewBehaviourFactory.CreateModel<CoinModel>(this).Initialize();

            _coinView = viewBehaviourFactory.CreateView<CoinView>(this).Initialize(_coinIconT, _coinTxt, _coinModel);
            
            _moveInOutAnim = viewBehaviourFactory.CreateViewLocalAnimation<MoveInOutAnimation>()
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
            _coinView?.Dispose();
        }
    }
}