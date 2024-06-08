namespace UnityBase.UI.ButtonCore
{
    public interface IButtonAction
    {
        public void OnClick();
        public void OnPointerDown();
        public void OnPointerUp();
        public void Dispose();
    }
}