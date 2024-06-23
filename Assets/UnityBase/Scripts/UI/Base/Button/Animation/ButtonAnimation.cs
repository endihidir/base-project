using Cysharp.Threading.Tasks;

namespace UnityBase.UI.ButtonCore
{
    public abstract class ButtonAnimation : IButtonAnimation
    {
        protected IButtonUI _buttonUI;
        protected ButtonAnimation(IButtonUI buttonUI) => _buttonUI = buttonUI;
        public abstract UniTask Click();
        public abstract UniTask PointerDown();
        public abstract UniTask PointerUp();
        public abstract void Dispose();
    }
    
    public interface IButtonAnimation
    {
        public UniTask Click();
        public UniTask PointerDown();
        public UniTask PointerUp();
        public void Dispose();
    }
}