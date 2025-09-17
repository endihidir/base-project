using TMPro;
using UnityBase.Service;
using UnityBase.UI.Config.SO;
using UnityBase.Runtime.Behaviours;
using UnityEngine;
using VContainer;

namespace UnityBase.UI.Dynamic
{
    public class CoinViewUI : DynamicUI
    {
        [SerializeField] private TextMeshProUGUI _coinTxt;
        [SerializeField] private Transform _coinIconT;
        [SerializeField] private MoveInOutViewConfigSO _moveInOutViewConfigSo;
        [SerializeField] private BounceViewConfigSO _bounceViewConfigSo;

        private IMoveInOutAnimation _moveInOutAnim;
        private ICoinCollectAnimation _coinCollectAnimation;
        private ICoinModel _coinModel;
        private ICoinView _coinView;
        private ICoinController _coinController;
        private IOwnerContext _coinOwnerContext;

        [Inject] 
        private ICurrencyManager _currencyManager;

        protected override void Initialize()
        {
            _coinOwnerContext = _ownerFactory.RegisterAndGetContext(this);

            _coinModel = _coinOwnerContext.CreateModel<CoinModel>().Initialize();
            _coinView  = _coinOwnerContext.CreateView<CoinView>().Initialize(_coinIconT, _coinTxt);
            _coinCollectAnimation = _coinOwnerContext.CreateAnimation<CoinCollectAnimation>();
            _moveInOutAnim = _coinOwnerContext.CreateAnimation<MoveInOutAnimation>()
                                          .Initialize(_rectTransform)
                                          .Configure(_moveInOutViewConfigSo);
            
            _currencyManager.BindCoinContext(_coinOwnerContext);
        }

        public override void OpenView() => _moveInOutAnim?.MoveIn();
        public override void CloseView() => _moveInOutAnim?.MoveOut();
        public override void OpenViewInstantly() => _moveInOutAnim?.MoveInInstantly();
        public override void CloseViewInstantly() => _moveInOutAnim?.MoveOutInstantly();

        protected override void OnDestroy()
        {
            _moveInOutAnim?.Dispose();
            _coinModel?.Dispose();
            _coinView?.Dispose();
            _coinCollectAnimation?.Dispose();

            _currencyManager?.UnbindCoinContext(_coinOwnerContext);
            
            _ownerFactory?.Release(this);

            _coinOwnerContext = null;
            _ownerFactory = null;
        }
    }
}
