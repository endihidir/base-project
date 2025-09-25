using UnityBase.Runtime.Factories;

namespace UnityBase.UI.ButtonCore
{
    public abstract class ButtonActionBase : IButtonAction
    {
        public abstract void OnClick();
        public abstract void OnPointerDown();
        public abstract void OnPointerUp();
        public abstract void Dispose();
    }
    
    public interface IButtonAction : IAction
    {
        public void OnClick();
        public void OnPointerDown();
        public void OnPointerUp();
    }
}