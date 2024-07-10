using TMPro;
using UnityBase.UI.Config.SO;
using UnityBase.UI.ViewCore;
using UnityEngine;

namespace UnityBase.UI.Dynamic
{
    public class LevelUI : DynamicUI
    {
        [SerializeField] private TextMeshProUGUI _levelTxt;

        [SerializeField] private MoveInOutViewConfigSO _moveInOutViewConfigSo;

        private IMoveInOutView _moveInOutAnim;

        private ILevelModel _levelModel;

        protected override void Initialize(IViewBehaviourFactory viewBehaviourFactory)
        {
            _levelModel = viewBehaviourFactory.CreateViewModel<LevelModel>(this)
                .Initialize(_levelTxt);

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
            _levelModel?.Dispose();
        }
    }
}