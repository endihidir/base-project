
namespace UnityBase.UI.ButtonCore
{
    public abstract class ButtonAction : IButtonAction
    {
        protected readonly IButtonUI _buttonUI;
        protected ButtonAction(IButtonUI buttonUI)
        {
            _buttonUI = buttonUI;
        }
        
        public abstract void OnClick();
        public abstract void OnPointerDown();
        public abstract void OnPointerUp();
        public abstract void Dispose();
    }
    
    public interface IButtonAction
    {
        public void OnClick();
        public void OnPointerDown();
        public void OnPointerUp();
        public void Dispose();
    }
}