using TMPro;
using UnityBase.UI.Config.SO;
using UnityBase.Runtime.Factories;
using UnityEngine;

namespace UnityBase.UI.Dynamic
{
    public class LevelUI : DynamicMonoView
    {
        [SerializeField] private TextMeshProUGUI _levelTxt;

        [SerializeField] private MoveInOutViewConfigSO _moveInOutViewConfigSo;

        private IMoveInOutAnimation _moveInOutAnim;

        private ILevelModel _levelModel;

        protected override void Initialize()
        {
            _levelModel = OwnerContext.ResolveModel<LevelModel>();

            _moveInOutAnim = OwnerContext.ResolveAnimation<MoveInOutAnimation>()
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
            base.OnDestroy();
            _moveInOutAnim?.Dispose();
            _levelModel?.Dispose();
        }
    }
}