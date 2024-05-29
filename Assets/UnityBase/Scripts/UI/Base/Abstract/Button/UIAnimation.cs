using Cysharp.Threading.Tasks;

namespace UnityBase.UI.ButtonCore
{
    public abstract class UIAnimation : IUIAnimation
    {
        protected readonly IButtonUI _buttonUI;
        protected UIAnimation(IButtonUI buttonUI) => _buttonUI = buttonUI;
        public abstract UniTask OpenAnimation();
        public abstract UniTask CloseAnimation();
        public abstract UniTask ClickAnimation();
        public abstract UniTask PointerDownAnimation();
        public abstract UniTask PointerUpAnimation();
        public abstract void Dispose();
    }
}