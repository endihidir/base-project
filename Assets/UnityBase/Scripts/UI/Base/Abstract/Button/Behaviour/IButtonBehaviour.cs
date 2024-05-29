namespace UnityBase.UI.ButtonCore
{
    public interface IButtonBehaviour
    {
        public bool IsClicked { get; }
        public IButtonAction ButtonAction { get; }
        public IUIAnimation UIAnimation { get; }
        public void ConfigureAction(params object[] parameters);
        public void ConfigureAnimation(params object[] parameters);
        public void OnClick();
        public void OnPointerDown();
        public void OnPointerUp();
        public void Dispose();
    }
}